using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pnnl.AppCore.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Pnnl.Api.Operations;

namespace Pnnl.Api.Approvals.Data.Sql
{
    public class SqlSecurityStore : SecurityStoreBase
    {
        private readonly ILogger<SqlSecurityStore> _logger;

        private readonly IOptions<SqlSecurityStoreOptions> _options;

        private readonly ISqlConnectionFactory _connectionFactory;

        public SqlSecurityStore(IMemoryCache cache, ILogger<SqlSecurityStore> logger, IOptions<SqlSecurityStoreOptions> options, ISqlConnectionFactory connectionFactory):base(cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override async Task<bool> OnIsSuperUserAsync(int processId, string userId, CancellationToken cancellationToken, IDictionary<object, object> context)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                        string query = $"SELECT dbo.FN_IS_PROCESS_SUPER_USER(@processId, @userId)";

                        var parameters = new DynamicParameters();
                        parameters.AddDynamicParams(new { processId, userId });

                        int temp = await connection.ExecuteScalarAsync<int>(query, parameters);

                        return temp == 1;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to check if the user with id {userId} is a super user. Reason: {ex}");
                throw;
            }
        }

        protected override async Task<bool> OnIsApprovalsAdminAsync( string userId, CancellationToken cancellationToken, IDictionary<object, object> context)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                        string query = $"SELECT dbo.fn_is_RAA_admin( @userId)";

                        var parameters = new DynamicParameters();
                        parameters.AddDynamicParams(new { userId });

                        int temp = await connection.ExecuteScalarAsync<int>(query, parameters);

                        return temp == 1;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to check if the user with id {userId} is a super user. Reason: {ex}");
                throw;
            }
        }

        // TO DO: Opwhse doesn't distribute SMA/GSMA accounts. This is a hack for the notable priority. A permanent solution to verify authority is needed instead of hard coding the value.
        protected override async Task<IList<string>> OnGetAuthorizedAccountsAsync(Process process, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            return await Task.Run(() =>
            {
                IList<string> validNetworkAccounts = new List<string>();

                // this is a stop gap for Activity Manager.  When the second service account needs access we need
                // to add a field on the process definition that contains the role name and then we need to pull
                // the account names out of the ad group.
                if (process.DocumentTypeName == "Activity Manager CSM Approval" || process.DocumentTypeName == "Activity Manager Worker Approval"|| process.DocumentTypeName == "Activity Manager Activity Approval")
                {
                    validNetworkAccounts.Add("msaActMan$");
                    validNetworkAccounts.Add("D3X213"); // Testing and stuff
                }

                //validNetworkAccounts.Add("D3X821"); // Testing and stuff

                return validNetworkAccounts;
            });
        }

        /// <summary>
        /// Asynchronously retrieves the list of document types that the specified user has the ability to view
        /// </summary>
        /// <param name="user">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        protected override async Task<IList<string>> OnGetReadDocumentTypesAsync(Person user, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {

                if (user.Network.Id == "msaActMan$")
                {
                    IList<string> types = new List<string>();

                    types.Add("Activity Manager CSM Approval");
                    types.Add("Activity Manager Worker Approval");
                    types.Add("Activity Manager Activity Approval");

                    return types;
                }
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {

                        string query = @"
IF (
        DBO.FN_IS_RAA_ADMIN(@id) = 1
        OR DBO.[fn_is_RAA_site_readonly](@id) = 1
        )
BEGIN
    SELECT DOC_TYPE_NAME
    FROM dbo.DEFN_PROCESS P
    WHERE P.IS_ENABLED  = 1
END
ELSE
BEGIN
    SELECT DOC_TYPE_NAME
    FROM dbo.DEFN_PROCESS P
    INNER JOIN opwhse.dbo.VW_PUB_RBAC_ROLE_ALL_MEMBERS M ON P.SUPER_USER_ROLE_NAME = M.PARENT_ROLE_NAME
    WHERE M.HANFORD_ID = @id
        AND P.IS_ENABLED  = 1
    UNION
        SELECT DOC_TYPE_NAME
    FROM dbo.DEFN_PROCESS P
    INNER JOIN opwhse.dbo.VW_PUB_RBAC_ROLE_ALL_MEMBERS M ON P.READONLY_USER_ROLE_NAME = M.PARENT_ROLE_NAME
    WHERE M.HANFORD_ID = @id
        AND P.IS_ENABLED  = 1
    UNION
    SELECT DOC_TYPE_NAME
    FROM dbo.DEFN_PROCESS P
    INNER JOIN opwhse.dbo.VW_PUB_RBAC_ROLE_ALL_MEMBERS M ON P.DEVELOPER_USER_ROLE_NAME = M.PARENT_ROLE_NAME
    WHERE M.HANFORD_ID = @id
        AND P.IS_ENABLED  = 1
END";


                        var parameters = new DynamicParameters();
                        parameters.AddDynamicParams(new { user.Id });

                        var temp = await connection.QueryAsync<string>(query, parameters);

                        return (IList<string>)temp;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to check if the user with id {user.Id} is a super user. Reason: {ex}");
                throw;
            }
        }


        /// <summary>
        /// Asynchronously returns a list of accounts that are authorized to make changes for that process definition   
        /// </summary>
        /// <param name="networkId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        protected override async Task<IList<string>> OnGetAuthorizedChangeAccountsAsync(int processDefinitionId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                        string sqlStmnt = @"
SELECT [NETWORK_ID] as NetworkId
FROM [RAA].[dbo].[AUTHORIZED_USER_ACCOUNTS]
WHERE DEFN_PROCESS_ID = @processDefinitionId
	AND ACCESS_TYPE = 'CHANGE'";
                        var parameters = new DynamicParameters();
                        parameters.AddDynamicParams(new { processDefinitionId = processDefinitionId });

                        //PersonIdentification personIdentification = null;

                        var temp = await connection.QueryAsync<string>(sqlStmnt, parameters);

                        if (temp == null) throw new System.Data.RowNotInTableException();

                        return (IList<string>)temp;
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve requested item. Reason: {ex}");
                throw;
            }
        }

    }
}
