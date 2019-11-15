using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Pnnl.AppCore.Data.Sql;
using Microsoft.Extensions.Options;
using System.Data;
using Microsoft.Extensions.Caching.Memory;

namespace Pnnl.Api.Approvals.Data.Sql
{
    /// <summary>
    /// The SQL data store to access the person information
    /// </summary>
    public class SqlPersonIdentificationStore : PersonIdentificationStoreBase
    {
        private readonly ILogger<SqlPersonIdentificationStore> _logger;
        private readonly IOptions<SqlPersonIdentificationStoreOptions> _options;
        private readonly ISqlConnectionFactory _connectionFactory;

        /// <summary>
        /// Constructor with basic parameters for dependency injection.
        /// </summary>
        /// <param name="cache">The cache for holding results for reuse.</param>
        /// <param name="logger">The logger for capturing logs.</param>
        /// <param name="options">Options for the store</param>
        /// <param name="connectionFactory">The connection factory used to generate connections for the applciation.</param>
        public SqlPersonIdentificationStore(IMemoryCache cache, ILogger<SqlPersonIdentificationStore> logger, IOptions<SqlPersonIdentificationStoreOptions> options, ISqlConnectionFactory connectionFactory):base(cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="employeeId">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PersonIdentification> OnGetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                        string sqlStmnt = @"
SELECT EMPLID AS EmployeeId
	, HANFORD_ID AS HanfordId
	, NETWORK_DOMAIN AS Domain
	, NETWORK_ID AS NetworkId
	, CASE WHEN ACTIVE_SW = 'Y' THEN
		1
		ELSE
		0
	END as IsActive
FROM [opwhse].[dbo].[VW_PUB_PERSON]
WHERE EMPLID =  @employeeId";
                        var parameters = new DynamicParameters();
                        parameters.AddDynamicParams(new { employeeId = employeeId });


                        var personIdentification = await connection.QuerySingleAsync<PersonIdentification>(sqlStmnt, parameters);

                    if (personIdentification == null)
                        throw new RowNotInTableException();

                        return personIdentification;
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

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="id">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PersonIdentification> OnGetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                    string sqlStmnt = @"
SELECT EMPLID as EmployeeId
	, HANFORD_ID as HanfordId
	, NETWORK_DOMAIN as Domain
	, NETWORK_ID as NetworkId
	, CASE WHEN ACTIVE_SW = 'Y' THEN
		1
		ELSE
		0
	END as IsActive
FROM [opwhse].[dbo].[VW_PUB_PERSON]
WHERE EMPLID = @id
	OR HANFORD_ID = @id
	OR NETWORK_ID = @id";
                    var parameters = new DynamicParameters();
                    parameters.AddDynamicParams(new { id });

                    var personIdentification = await connection.QuerySingleAsync<PersonIdentification>(sqlStmnt, parameters);

                    return personIdentification;
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

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="hanfordId">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PersonIdentification> OnGetByHanfordIdAsync(string hanfordId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                    string sqlStmnt = @"
SELECT EMPLID AS EmployeeId
	, HANFORD_ID AS HanfordId
	, NETWORK_DOMAIN AS Domain
	, NETWORK_ID AS NetworkId
	, CASE WHEN ACTIVE_SW = 'Y' THEN
		1
		ELSE
		0
	END as IsActive
FROM [opwhse].[dbo].[VW_PUB_PERSON]
WHERE HANFORD_ID =  @hanfordId";
                    var parameters = new DynamicParameters();
                    parameters.AddDynamicParams(new { hanfordId });

                    var personIdentification = await connection.QuerySingleAsync<PersonIdentification>(sqlStmnt, parameters);

                    return personIdentification;
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

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="domain">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="networkId">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PersonIdentification> OnGetByNetworkIdAsync(string domain, string networkId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                    string sqlStmnt = @"
SELECT EMPLID AS EmployeeId
	, HANFORD_ID AS HanfordId
	, NETWORK_DOMAIN AS Domain
	, NETWORK_ID AS NetworkId
	, CASE WHEN ACTIVE_SW = 'Y' THEN
		1
		ELSE
		0
	END as IsActive
FROM [opwhse].[dbo].[VW_PUB_PERSON]
WHERE NETWORK_DOMAIN =  @domain
    AND NETWORK_ID =  @networkId";
                    var parameters = new DynamicParameters();
                    parameters.AddDynamicParams(new { domain, networkId });

                    var personIdentification = await connection.QuerySingleAsync<PersonIdentification>(sqlStmnt, parameters);

                    return personIdentification;
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
