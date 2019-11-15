using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pnnl.AppCore.Data.Sql;
using Microsoft.Extensions.Options;
using System.Data;
using Pnnl.Data.Paging;
using System.Text;
using Pnnl.Api.Approvals.Data.Interfaces;

namespace Pnnl.Api.Approvals.Data.Sql
{
    /// <summary>
    /// The SQL data store to access the process information
    /// </summary>
    public class SqlProcessStore : ProcessStoreBase
    {
        private readonly ILogger<SqlProcessStore> _logger;
        private readonly IOptions<SqlProcessStoreOptions> _options;
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IPersonIdentificationStore _personIdentificationStore;

        /// <summary>
        /// Constructor with basic parameters for dependency injection.
        /// </summary>
        /// <param name="logger">The logger for capturing logs.</param>
        /// <param name="options">Options for the store</param>
        /// <param name="connectionFactory">The connection factory used to generate connections for the applciation.</param>
        /// <param name="personIdentificationStore">Can be used to map between emplid, hanford Id and Network id.</param>
        public SqlProcessStore(ILogger<SqlProcessStore> logger, IOptions<SqlProcessStoreOptions> options, ISqlConnectionFactory connectionFactory, IPersonIdentificationStore personIdentificationStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _personIdentificationStore = personIdentificationStore ?? throw new ArgumentNullException(nameof(personIdentificationStore));
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Process"/>.
        /// </summary>
        /// <param name="processIds">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PagedResult<Process>> OnGetAsync(IList<int> processIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                //int count = await GetProcessCountAsync(processIds);
                int count = processIds.Count;

                List<int> subList = processIds
                                        .Skip(offset)
                                        .Take(limit)
                                        .ToList();

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                {
                    IDictionary<int, Process> processList = await GetProcessList(subList, connection);

                    processList = PopulateItemsNotFound(subList, processList);

                    return new PagedResult<Process>(offset, limit, count, processList.Values.ToList());
                }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve process list. Reason: {ex}");
                throw;
            }
        }

        private IDictionary<int, Process> PopulateItemsNotFound(List<int> subList, IDictionary<int, Process> processList)
        {
            const string errMsg = "ITEM NOT FOUND";

            foreach (int id in subList)
            {
                if (!processList.ContainsKey(id))
                {
                    processList.Add(id, new Process() {
                        ProcessId = id,
                        ProcessState = errMsg,
                        ProcessStatus = errMsg
                    });
                }
            }

            return processList;
        }

        private static async Task<IDictionary<int, Process>> GetProcessList(List<int> subList, SqlConnection cn)
        {
            string sqlStmnt = @"
SELECT DISTINCT P.[PROCESS_ID]                                            AS ProcessId,
            P.[DEFN_PROCESS_ID]                                         AS ProcessDefinitionId,
            P.DOC_TYPE_NAME                                             AS DocumentTypeName,
            P.DOC_ID                                                    AS DocumentId,
            P.DOC_AT_A_GLANCE                                           AS DocumentTitle,
            P.DOC_ORIGINATOR_ID                                         AS OriginatorId,
            P.DOC_BENEFICIARY_ID                                        AS BeneficiaryId,
            P.STATE                                                     AS ProcessState,
            P.STATUS                                                    AS ProcessStatus,
            P.CREATE_DATETIME                                           AS CreateDateTime,
            P.LAST_CHANGE_DATETIME                                      AS LastChangeDateTime,
            Isnull(Cast(H.HANFORD_ID AS VARCHAR(20)), P.LAST_CHANGE_ID) AS LastChangeHanfordId,
            A.ACTIVITY_ID                                               AS ActivityId,
            A.ACTIVITY_NAME                                             AS ActivityName,
            A.PENDING_DTM                                               AS PendingDateTime,
            A.LAST_CHANGE_DATETIME                                      AS LastChangeDateTime,
            a.STATE                                                     AS ActivityState,
            A.STATUS                                                    AS ActivityStatus,
            A.COMMENT                                                   AS Comment,
            A.ACTED_USERID                                              AS ActedUserId,
            AA.HANFORD_ID                                               AS ActedHanfordId,
            A.ACTED_ACTOR_ID                                            AS ActedActorId,
            A.IS_GHOST                                                  AS IsGhost,
            A.IS_ADHOC                                                  AS IsAdhoc,
            ACT.ACTOR_ID                                                AS ActorId,
            ACT.ACTOR_CRITERIA_ID                                       AS ActorCriteriaId,
            ACT.ACTIVITY_ID                                             AS ActivityId,
            AH.HANFORD_ID                                               AS ActorHanfordId,
            ACT.ACTOR_TYPE                                              AS ActorType,
            AD.HANFORD_ID                                               AS DelegatorHanfordId
FROM   RAA.dbo.PROCESS P
LEFT OUTER JOIN opwhse.dbo.vw_pub_person H
        ON P.LAST_CHANGE_ID = H.EMPLID
INNER JOIN RAA.dbo.ACTIVITY A
		ON P.PROCESS_ID = A.PROCESS_ID
LEFT OUTER JOIN opwhse.dbo.vw_pub_person AA
        ON A.ACTED_USERID = AA.EMPLID
LEFT OUTER JOIN RAA.dbo.ACTOR ACT
        ON A.ACTIVITY_ID = ACT.ACTIVITY_ID
LEFT OUTER JOIN opwhse.dbo.vw_pub_person AH
        ON ACT.EMPLID = AH.EMPLID
LEFT OUTER JOIN opwhse.dbo.vw_pub_person AD
        ON ACT.DELEGATOR_ID = AD.EMPLID
WHERE  P.PROCESS_ID in @Ids

UNION

SELECT DISTINCT P.[PROCESS_ID]                                              AS ProcessId,
                0                                                           AS ProcessDefinitionId,
                P.DOC_TYPE_NAME                                             AS DocumentTypeName,
                P.DOC_ID                                                    AS DocumentId,
                P.DOC_AT_A_GLANCE                                           AS DocumentTitle,
                P.DOC_ORIGINATOR_ID                                         AS OriginatorId,
                P.DOC_BENEFICIARY_ID                                        AS BeneficiaryId,
                'QUEUED'                                                    AS ProcessState,
                NULL                                                        AS ProcessStatus,
                P.CREATE_DATETIME                                           AS CreateDateTime,
                P.LAST_CHANGE_DATETIME                                      AS LastChangeDateTime,
                Isnull(Cast(H.HANFORD_ID AS VARCHAR(20)), P.LAST_CHANGE_ID) AS LastChangeHanfordId,
                0                                                           AS ActivityId,
                NULL                                                        AS ActivityName,
                NULL                                                        AS PendingDateTime,
                NULL                                                        AS LastChangeDateTime,
                NULL                                                        AS ActivityState,
                NULL                                                        AS ActivityStatus,
                NULL                                                        AS Comment,
                NULL                                                        AS ActedUserId,
                NULL                                                        AS ActedHanfordId,
                NULL                                                        AS ActedActorId,
                0															AS IsGhost,
                0															AS IsAdhoc,
                0                                                           AS ActorId,
                0                                                           AS ActorCriteriaId,
                0                                                           AS ActivityId,
                CAST(0 AS varchar(7))										AS ActorHanfordId,
                NULL                                                        AS ActorType,
                CAST(0 AS varchar(7))										AS DelegatorHanfordId
FROM   RAA.dbo.PROCESS_QUEUE P
LEFT OUTER JOIN opwhse.dbo.vw_pub_person H
        ON P.LAST_CHANGE_ID = H.EMPLID 
WHERE  P.PROCESS_ID in @Ids";

            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(new { Ids = subList });

            IDictionary<int, Process> processList = new Dictionary<int, Process>(subList.Count);

            await cn.QueryAsync(sqlStmnt
                , (Process proc, Activity a, Actor act) =>
                {
                    if (!processList.TryGetValue(proc.ProcessId, out Process process))
                    {
                        processList.Add(proc.ProcessId, process = proc);
                    }

                    if (a.ActivityId != 0)
                    {
                        if (process.Activities == null)
                            process.Activities = new Dictionary<int, Activity>();

                        if (!process.Activities.TryGetValue(a.ActivityId, out Activity activity))
                        {
                            process.Activities.Add(a.ActivityId, activity = a);
                        }

                        if (activity.Actors == null)
                            activity.Actors = new Dictionary<int, Actor>();

                        // If an activity has been ghosted it will not have an actors assigned to it.
                        if (act != null)
                        {
                            if (!activity.Actors.TryGetValue(act.ActorId, out Actor actor))
                            {
                                activity.Actors.Add(act.ActorId, actor = act);
                            }
                        }
                    }
                    return process;
                }
                , parameters
                , splitOn: "ProcessId,ActivityId,ActorId"
                , commandType: CommandType.Text);

            return processList;
        }

        private async Task<IDictionary<int, Process>> GetProcessList(string query, DynamicParameters parameters, int offset, int limit, SqlConnection cn, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            IDictionary<int, Process> processList = new Dictionary<int, Process>(limit);

            parameters.Add("offset", offset);
            parameters.Add("limit", limit);

            await cn.QueryAsync(query
                , (Process proc, Activity a, Actor act) =>
                {
                    if (!processList.TryGetValue(proc.ProcessId, out Process process))
                    {
                        processList.Add(proc.ProcessId, process = proc);
                    }

                    if (process.Activities == null)
                        process.Activities = new Dictionary<int, Activity>();

                    if (!process.Activities.TryGetValue(a.ActivityId, out Activity activity))
                    {
                        process.Activities.Add(a.ActivityId, activity = a);
                    }

                    if (activity.Actors == null)
                        activity.Actors = new Dictionary<int, Actor>();

                    // If an activity has been ghosted it will not have an actors assigned to it.
                    if (act != null)
                    {
                        if (!activity.Actors.TryGetValue(act.ActorId, out Actor actor))
                        {
                            activity.Actors.Add(act.ActorId, actor = act);
                        }
                    }
                    return process;
                }
                , parameters
                , splitOn: "ProcessId,ActivityId,ActorId"
                , commandType: CommandType.Text);


            return processList;
        }

        /// <summary>
        /// Asynchronously retrieves a count of <see cref="Process"/>.
        /// </summary>
        /// <param name="query">The that defines the total result set without pagination.</param>
        /// <param name="parameters">The that will filter the result set.</param>
        /// <param name="connection">The connection to the sql database.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        private async Task<int> GetProcessCountAsync(string query, DynamicParameters parameters, SqlConnection connection, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                int count = 0;
                count = await connection.ExecuteScalarAsync<int>(query, parameters);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve process list. Reason: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a count of <see cref="Process"/>.
        /// </summary>
        /// <param name="processIds">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        private async Task<int> GetProcessCountAsync(IList<int> processIds, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                {
                    string sqlStmnt = @"
SELECT COUNT(P.[PROCESS_ID]) AS ProcessCount
FROM RAA.dbo.PROCESS P
WHERE P.PROCESS_ID in @Ids";
                    var parameters = new DynamicParameters();
                    parameters.AddDynamicParams(new { Ids = processIds });

                    int count = 0;

                    count = await connection.ExecuteScalarAsync<int>(sqlStmnt, parameters);

                    //IPagedResult<int, Process> returnResult = new PagedResult<Dictionary<int, Process>>(offset, limit, 0, processList);
                    return count;
                }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve process list. Reason: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Process"/>.
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PagedResult<Process>> OnSearchWithActorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                IList<string> activityStateList = BuildActivityStateList(processFilter);
                IList<string> processStateList = BuildProcessStateList(processFilter);
                IList<string> typeList = BuildTypeList(processFilter);
                IList<string> originatorList = await BuildLOriginatorList(processFilter);
                IList<string> beneficiaryList = await BuildLBeneficiaryList(processFilter);

                string query = BuildActorQueryClause(activityStateList, processStateList, processFilter.DocumentTypeList, originatorList, beneficiaryList, processFilter.CreateDateRange != null, processFilter.LastChangeDateRange != null);

                DynamicParameters parameters = BuildActorParameters(processFilter, activityStateList, processStateList, typeList, originatorList, beneficiaryList);

                return await ExecuteSearch(offset, limit, context, query, parameters, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve process list. Reason: {ex}");
                throw;
            }
        }

        private async Task<PagedResult<Process>> ExecuteSearch(int offset, int limit, IDictionary<object, object> context, string query, DynamicParameters parameters, CancellationToken cancellationToken)
        {
            var policy = _connectionFactory.PolicySelector(_logger);

            var result = await policy.ExecuteAsync(async () =>
            {
                using (var connection = _connectionFactory.Create("raa"))
            {
                int count = await GetProcessCountAsync(BuildCountQuery(query), parameters, connection, cancellationToken, context);

                IDictionary<int, Process> processList = await GetProcessList(BuildResultSetQuery(query), parameters, offset, limit, connection, cancellationToken, context);

                foreach (var processId in processList.Keys)
                {
                    var process = processList[processId];

                    PersonIdentification org = await _personIdentificationStore.GetByIdAsync(process.OriginatorId);
                    process.OriginatorHanfordId = org.HanfordId;

                    PersonIdentification beni = await _personIdentificationStore.GetByIdAsync(process.BeneficiaryId);
                    process.BeneficiaryHanfordId = beni.HanfordId;

                    foreach(var activity in process.Activities)
                    {
                        if(!string.IsNullOrEmpty(activity.Value.ActedUserId))
                        {
                            PersonIdentification act = await _personIdentificationStore.GetByIdAsync(activity.Value.ActedUserId);
                            activity.Value.ActedHanfordId = act.HanfordId;
                        }
                    }
                }

                return new PagedResult<Process>(offset, limit, count, processList.Values.ToList());
            }
            });
            return result;
        }

        private static DynamicParameters BuildActorParameters(ProcessFilter processFilter, IList<string> activityStateList, IList<string> processStateList, IList<string> typeList, IList<string> originatorList, IList<string> beneficiaryList)
        {
            DynamicParameters parameters = new DynamicParameters();

            // only required field
            parameters.Add("actorHanfordId", processFilter.ActorIdList[0]);

            if (typeList != null)
                parameters.Add("docTypeList", typeList.ToArray());

            if (activityStateList != null)
                parameters.Add("activityStateList", activityStateList);

            if (processStateList != null)
                parameters.Add("processStateList", processStateList);

            if (originatorList != null)
                parameters.Add("originatorId", originatorList);

            if (beneficiaryList != null)
                parameters.Add("beneficiaryId", beneficiaryList);

            if (processFilter.CreateDateRange != null)
            {
                parameters.Add("createStart", processFilter.CreateDateRange.Start);
                parameters.Add("createEnd", processFilter.CreateDateRange.End);
            }

            if (processFilter.CreateDateRange != null)
            {
                parameters.Add("lastChangeStart", processFilter.LastChangeDateRange.Start);
                parameters.Add("lastChangeEnd", processFilter.LastChangeDateRange.End);
            }

            return parameters;
        }

        private DynamicParameters BuildUserParameters(ProcessFilter processFilter, IList<string> activityStateList, IList<string> processStateList, IList<string> typeList, IList<string> originatorList, IList<string> beneficiaryList)
        {
            DynamicParameters parameters = new DynamicParameters();

            if(NeedsCriteria(processFilter.ActorIdList))
                parameters.Add("actorList", processFilter.ActorIdList);

            if(NeedsCriteria(processFilter.DocumentTypeList))
                parameters.Add("docTypeList", processFilter.DocumentTypeList);

            if (activityStateList != null)
                parameters.Add("activityStateList", activityStateList);

            if (processStateList != null)
                parameters.Add("processStateList", processStateList);

            if (originatorList != null)
                parameters.Add("originatorId", originatorList);

            if (beneficiaryList != null)
                parameters.Add("beneficiaryId", beneficiaryList);

            if (processFilter.CreateDateRange != null)
            {
                parameters.Add("createStart", processFilter.CreateDateRange.Start);
                parameters.Add("createEnd", processFilter.CreateDateRange.End);
            }

            if (processFilter.LastChangeDateRange != null)
            {
                parameters.Add("lastChangeStart", processFilter.LastChangeDateRange.Start);
                parameters.Add("lastChangeEnd", processFilter.LastChangeDateRange.End);
            }

            return parameters;
        }

        private DynamicParameters BuildOriginatorParameters(ProcessFilter processFilter, IList<string> activityStateList, IList<string> processStateList, IList<string> typeList, IList<string> originatorList, IList<string> beneficiaryList)
        {
            DynamicParameters parameters = new DynamicParameters();

            if(NeedsCriteria(processFilter.ActorIdList))
                parameters.Add("actorList", processFilter.ActorIdList);

            if(NeedsCriteria(processFilter.DocumentTypeList))
                parameters.Add("docTypeList", processFilter.DocumentTypeList);

            if (activityStateList != null)
                parameters.Add("activityStateList", activityStateList);

            if (processStateList != null)
                parameters.Add("processStateList", processStateList);

            if (originatorList != null)
                parameters.Add("originatorId", originatorList);

            if (beneficiaryList != null)
                parameters.Add("beneficiaryId", beneficiaryList);

            if (processFilter.CreateDateRange != null)
            {
                parameters.Add("createStart", processFilter.CreateDateRange.Start);
                parameters.Add("createEnd", processFilter.CreateDateRange.End);
            }

            if (processFilter.CreateDateRange != null)
            {
                parameters.Add("lastChangeStart", processFilter.LastChangeDateRange.Start);
                parameters.Add("lastChangeEnd", processFilter.LastChangeDateRange.End);
            }

            return parameters;
        }

        private List<string> BuildActivityStateList(ProcessFilter processFilter)
        {
            List<string> stateList = null;

            if (processFilter.ActivityStateList != null && processFilter.ActivityStateList.Count > 0)
            {
                stateList = new List<string>();

                foreach (ActivityState state in processFilter.ActivityStateList)
                {
                    stateList.Add(state.Name);
                }
            }

            return stateList;
        }

        private List<string> BuildProcessStateList(ProcessFilter processFilter)
        {
            List<string> processStateList = null;

            if( processFilter.ProcessStateList != null && processFilter.ProcessStateList.Count > 0)
            {
                processStateList = new List<string>();

                foreach(ProcessState state in processFilter.ProcessStateList)
                {
                    processStateList.Add(state.Name);
                }                
            }

            return processStateList;
        }

        private bool NeedsCriteria<T>(List<T> list)
        {
            return list != null && list.Count > 0;
        }

        private List<string> BuildTypeList(ProcessFilter processFilter)
        {
            if (processFilter.DocumentTypeList != null && processFilter.DocumentTypeList.Count > 0)
            {
                return processFilter.DocumentTypeList;
            }
            else
            {
                return null;
            }
        }

        private async Task<List<string>> BuildLOriginatorList(ProcessFilter processFilter)
        {
            List<string> originatorList = null;

            if (processFilter.OriginatorIdList != null && processFilter.OriginatorIdList.Count > 0)
            {
                originatorList = new List<string>();

                foreach (string originator in processFilter.OriginatorIdList)
                {
                    originatorList.Add(originator);

                    var person = await _personIdentificationStore.GetByHanfordIdAsync(originator);

                    originatorList.Add(person.EmployeeId);
                }
            }

            return originatorList;
        }

        private async Task<List<string>> BuildLBeneficiaryList(ProcessFilter processFilter)
        {
            List<string> beneficiaryList = null;

            if (processFilter.BeneficiaryIdList != null && processFilter.BeneficiaryIdList.Count > 0)
            {
                beneficiaryList = new List<string>();

                foreach (string beneficiary in processFilter.BeneficiaryIdList)
                {
                    beneficiaryList.Add(beneficiary);

                    var person = await _personIdentificationStore.GetByHanfordIdAsync(beneficiary);

                    beneficiaryList.Add(person.EmployeeId);
                }
            }

            return beneficiaryList;
        }

        private async Task<List<string>> GetSingleUserIdList(List<string> userList)
        {
            List<string> userIds = null;

            if (userList != null && userList.Count == 1)
            {
                string tempUser = userList[0];

                userIds = new List<string>();
                var person = await _personIdentificationStore.GetByHanfordIdAsync(tempUser);

                userIds.Add(person.EmployeeId);
                userIds.Add(person.HanfordId);
            }

            return userIds;
        }

        private string BuildActorQueryClause(IList<string> activityStateList, IList<string> processStateList, IList<string> docTypeList, IList<string> originatorList, IList<string> beneficiaryList, bool needsCreate, bool needsLastChange)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("SELECT DISTINCT P.[PROCESS_ID] AS ProcessId");
            queryBuilder.AppendLine("    FROM RAA.dbo.PROCESS P");
            queryBuilder.AppendLine("    INNER JOIN (");
            queryBuilder.AppendLine("        SELECT PROCESS_ID");
            queryBuilder.AppendLine("        FROM ACTOR ACT");
            queryBuilder.AppendLine("        INNER JOIN ACTIVITY A ON ACT.ACTIVITY_ID = A.ACTIVITY_ID");

            if (activityStateList != null)
                queryBuilder.AppendLine("        AND STATE IN @activityStateList");

            queryBuilder.AppendLine("            WHERE ACT.HANFORD_ID = @actorHanfordId");
            queryBuilder.AppendLine("            ) R ON P.PROCESS_ID = R.PROCESS_ID");

            if (originatorList != null || docTypeList != null || beneficiaryList != null || needsCreate || needsLastChange)
            {
                queryBuilder.AppendLine("        WHERE ");

                IList<string> clauses = new List<string>();

                if (processStateList != null)
                    clauses.Add("P.STATE in @processStateList");

                if (docTypeList != null)
                    clauses.Add("P.DOC_TYPE_NAME in @docTypeList");

                if (originatorList != null)
                    clauses.Add("P.DOC_ORIGINATOR_ID in @originatorId");

                if (beneficiaryList != null)
                    clauses.Add("P.DOC_BENEFICIARY_ID in @beneficiaryId");

                if (needsCreate)
                    clauses.Add("P.CREATE_DATETIME between @createStart and @createEnd");

                if (needsLastChange)
                    clauses.Add("P.LAST_CHANGE_DATETIME between @lastChangeStart and @lastChangeEnd");

                queryBuilder.Append(string.Join(" AND ", clauses));
            }

            return queryBuilder.ToString();
        }

        private string BuildUserQueryClause(ProcessFilter filter, bool needsCreate, bool needsLastChange)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("SELECT DISTINCT P.[PROCESS_ID] AS ProcessId");
            queryBuilder.AppendLine("FROM RAA.dbo.PROCESS P");
            queryBuilder.AppendLine("INNER JOIN (SELECT DEFN_PROCESS_ID FROM RAA.dbo.DEFN_PROCESS WHERE DOC_TYPE_NAME IN @docTypeList) D on P.DEFN_PROCESS_ID = D.DEFN_PROCESS_ID");

            if(NeedsCriteria(filter.ActorIdList))
            {
                queryBuilder.AppendLine("INNER JOIN (");
                queryBuilder.AppendLine("    SELECT PROCESS_ID");
                queryBuilder.AppendLine("    FROM ACTOR ACT");
                queryBuilder.AppendLine("    INNER JOIN ACTIVITY A ON ACT.ACTIVITY_ID = A.ACTIVITY_ID");
                if (NeedsCriteria(filter.ActivityStateList))
                    queryBuilder.AppendLine("    AND STATE IN @activityStateList");
                queryBuilder.AppendLine("    WHERE ACT.HANFORD_ID IN @actorList");
                queryBuilder.AppendLine("    ) R ON P.PROCESS_ID = R.PROCESS_ID");
            }

            if (NeedsCriteria(filter.OriginatorIdList) || NeedsCriteria(filter.BeneficiaryIdList) || needsCreate || needsLastChange || NeedsCriteria(filter.ProcessStateList))
            {
                queryBuilder.AppendLine("        WHERE ");

                List<string> clauses = new List<string>();

                if (NeedsCriteria(filter.ProcessStateList))
                    clauses.Add("P.STATE in @processStateList");

                if (NeedsCriteria(filter.OriginatorIdList))
                    clauses.Add("P.DOC_ORIGINATOR_ID in @originatorId");

                if (NeedsCriteria(filter.BeneficiaryIdList))
                    clauses.Add("P.DOC_BENEFICIARY_ID in @beneficiaryId");

                if (needsCreate)
                    clauses.Add("P.CREATE_DATETIME between @createStart and @createEnd");

                if (needsLastChange)
                    clauses.Add("P.LAST_CHANGE_DATETIME between @lastChangeStart and @lastChangeEnd");

                queryBuilder.Append(string.Join(" AND ", clauses));
            }

            return queryBuilder.ToString();
        }

        private string BuildOrginatorQueryClause(ProcessFilter filter, bool needsCreate, bool needsLastChange)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("SELECT DISTINCT P.[PROCESS_ID] AS ProcessId");
            queryBuilder.AppendLine("FROM RAA.dbo.PROCESS P");

            if (NeedsCriteria(filter.DocumentTypeList))
            {
                queryBuilder.AppendLine("INNER JOIN (SELECT DEFN_PROCESS_ID FROM RAA.dbo.DEFN_PROCESS WHERE DOC_TYPE_NAME IN @docTypeList) D on P.DEFN_PROCESS_ID = D.DEFN_PROCESS_ID");
            }

            if(NeedsCriteria(filter.ActorIdList))
            {
                queryBuilder.AppendLine("INNER JOIN (");
                queryBuilder.AppendLine("    SELECT PROCESS_ID");
                queryBuilder.AppendLine("    FROM ACTOR ACT");
                queryBuilder.AppendLine("    INNER JOIN ACTIVITY A ON ACT.ACTIVITY_ID = A.ACTIVITY_ID");
                if (NeedsCriteria(filter.ActivityStateList))
                    queryBuilder.AppendLine("    AND STATE IN @activityStateList");
                queryBuilder.AppendLine("    WHERE ACT.HANFORD_ID IN @actorList");
                queryBuilder.AppendLine("    ) R ON P.PROCESS_ID = R.PROCESS_ID");
            }

            if (NeedsCriteria(filter.OriginatorIdList) || NeedsCriteria(filter.BeneficiaryIdList) || needsCreate || needsLastChange)
            {
                queryBuilder.AppendLine("        WHERE ");

                List<string> clauses = new List<string>();

                if (NeedsCriteria(filter.ProcessStateList))
                    clauses.Add("P.STATE in @processStateList");

                if (NeedsCriteria(filter.OriginatorIdList))
                    clauses.Add("P.DOC_ORIGINATOR_ID in @originatorId");

                if (NeedsCriteria(filter.BeneficiaryIdList))
                    clauses.Add("P.DOC_BENEFICIARY_ID in @beneficiaryId");

                if (needsCreate)
                    clauses.Add("P.CREATE_DATETIME between @createStart and @createEnd");

                if (needsLastChange)
                    clauses.Add("P.LAST_CHANGE_DATETIME between @lastChangeStart and @lastChangeEnd");

                queryBuilder.Append(string.Join(" AND ", clauses));
            }

            return queryBuilder.ToString();
        }

        private string BuildResultSetQuery(string processListQuery)
        {
            string query = $@"  SELECT DISTINCT P.[PROCESS_ID] AS ProcessId
                                    ,P.[DEFN_PROCESS_ID] AS ProcessDefinitionId
                                    ,P.DOC_TYPE_NAME AS DocumentTypeName
                                    ,P.DOC_ID AS DocumentId
                                    ,P.DOC_AT_A_GLANCE AS DocumentTitle
                                    ,P.DOC_ORIGINATOR_ID AS OriginatorId
                                    ,P.DOC_BENEFICIARY_ID AS BeneficiaryId
                                    ,P.STATE AS ProcessState
                                    ,P.STATUS AS ProcessStatus
                                    ,P.CREATE_DATETIME AS CreateDateTime
                                    ,P.LAST_CHANGE_DATETIME AS LastChangeDateTime
                                    ,P.LAST_CHANGE_ID AS LastChangeId
                                    ,A.ACTIVITY_ID AS ActivityId
                                    ,A.ACTIVITY_NAME AS ActivityName
                                    ,A.PENDING_DTM AS PendingDateTime
                                    ,A.LAST_CHANGE_DATETIME AS LastChangeDateTime
                                    ,a.STATE AS ActivityState
                                    ,A.STATUS AS ActivityStatus
                                    ,A.COMMENT AS Comment
                                    ,A.ACTED_ACTOR_ID AS ActedActorId
                                    ,A.ACTED_USERID AS ActedUserId
                                    ,A.IS_GHOST AS IsGhost
                                    ,A.IS_ADHOC AS IsAdhoc
                                    ,ACT.ACTOR_ID AS ActorId
                                    ,ACT.ACTOR_CRITERIA_ID AS ActorCriteriaId
                                    ,ACT.ACTIVITY_ID AS ActivityId
                                    ,ACT.HANFORD_ID  AS ActorHanfordId
                                    ,ACT.ACTOR_TYPE AS ActorType
                                    ,ACT.DELEGATOR_ID AS DelegatorId
                                FROM RAA.dbo.PROCESS P
                                    INNER JOIN RAA.dbo.ACTIVITY A 
                                        ON P.PROCESS_ID = A.PROCESS_ID
                                    LEFT OUTER JOIN RAA.dbo.ACTOR ACT 
                                        ON A.ACTIVITY_ID = ACT.ACTIVITY_ID
                                WHERE P.PROCESS_ID IN ( { processListQuery }
                                                        ORDER BY P.PROCESS_ID OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY )";

            return query;
        }

        private string BuildCountQuery(string processListQuery)
        {
            string query = $@"  SELECT COUNT (C.[PROCESS_ID])
                                FROM RAA.dbo.PROCESS C
                                WHERE C.PROCESS_ID IN ( { processListQuery } )";

            return query;
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Process"/>.
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PagedResult<Process>> OnSearchWithOriginatorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                IList<string> activityStateList = BuildActivityStateList(processFilter);
                IList<string> processStateList = BuildProcessStateList(processFilter);
                IList<string> typeList = BuildTypeList(processFilter);
                IList<string> originatorList = await GetSingleUserIdList(processFilter.OriginatorIdList);
                IList<string> beneficiaryList = await GetSingleUserIdList(processFilter.BeneficiaryIdList);

                string query = BuildOrginatorQueryClause( processFilter, processFilter.CreateDateRange != null, processFilter.LastChangeDateRange != null);

                DynamicParameters parameters = BuildOriginatorParameters(processFilter, activityStateList, processStateList, typeList, originatorList, beneficiaryList);

                return await ExecuteSearch(offset, limit, context, query, parameters, cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve process list. Reason: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Process"/>.
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PagedResult<Process>> OnSearchWithUserAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                IList<string> activityStateList = BuildActivityStateList(processFilter);
                IList<string> processStateList = BuildProcessStateList(processFilter);
                IList<string> typeList = BuildTypeList(processFilter);
                IList<string> originatorList = await BuildLOriginatorList(processFilter);
                IList<string> beneficiaryList = await BuildLBeneficiaryList(processFilter);

                string query = BuildUserQueryClause( processFilter, processFilter.CreateDateRange != null, processFilter.LastChangeDateRange != null);

                DynamicParameters parameters = BuildUserParameters(processFilter, activityStateList, processStateList,  typeList, originatorList, beneficiaryList);

                return await ExecuteSearch(offset, limit, context, query, parameters, cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve process list. Reason: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously terminates the <see cref="Process"/> instance based on the unique identifier provided
        /// </summary>
        /// <param name="processId">The unquie identifier of the process to be terminated.</param>
        /// <param name="terminatingUserId">The person making the request for the process to be terminated.</param>
        /// <param name="processStatus">The new status of the process.</param>
        /// <param name="sendNotifications">Whether or not to send notifications.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The <see cref="Process"/> after the change has been applied.</returns>
        protected override async Task<Process> OnTerminateAsync( int processId, string terminatingUserId, string processStatus, bool sendNotifications, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {

            cancellationToken.ThrowIfCancellationRequested();

            //DynamicParameters parameters;
            //string query;
            var policy = _connectionFactory.PolicySelector(_logger);

            var result = await policy.ExecuteAsync(async () =>
            {
                using (var connection = _connectionFactory.Create("raa"))
            {
                    IDbTransaction transaction = connection.BeginTransaction();

                try
                {
                        await SetProcessTerminated(processId, terminatingUserId, processStatus, connection, transaction);

                        await ClearActivityState(processId, terminatingUserId, connection, transaction);

                    if (sendNotifications)
                    {
                            await CreateTerminatedNotification(processId, terminatingUserId, connection, transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError($"Unable to retrieve process list. Reason: {ex}");
                    throw;
                }
            }

            // we don't have to do it like this, but this keeps the columns consistent when we populate the object
            IList<int> ids = new List<int>() { processId };
            string query = BuildListClause();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Ids", ids);

            return (await ExecuteSearch(0, 50, context, query, parameters, cancellationToken)).FirstOrDefault();
            });
            return result;

        }

        private static async Task CreateTerminatedNotification(int processId, string terminatingUserId, SqlConnection cn, IDbTransaction transaction)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("process_id", processId);
            parameters.Add("state", ProcessState.Terminated.Name);
            parameters.Add("terminatingUserId", terminatingUserId);

            string query = @"EXEC dbo.up_create_email_item_completed @process_id
                        , @state
                        , @terminatingUserId";

            await cn.ExecuteAsync(query, parameters, transaction);
        }

        private static async Task ClearActivityState(int processId, string terminatingUserId, SqlConnection cn, IDbTransaction transaction)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("process_id", processId);
            parameters.Add("terminatingUserId", terminatingUserId);

            string query = @"Update dbo.ACTIVITY
                               Set STATE                = Null,
                                   LAST_CHANGE_ID       = @terminatingUserId,
                                   LAST_CHANGE_DATETIME = getdate()
                             Where STATE                in( 'PENDING','PENDING DIGSIG','PENDING ESCALATED')
                               And PROCESS_ID           = @process_id ";

            await cn.ExecuteAsync(query, parameters, transaction);
        }

        private static async Task SetProcessTerminated(int processId, string terminatingUserId, string processStatus, SqlConnection cn, IDbTransaction transaction)
        {
            string state = ProcessState.Terminated.Name;

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("process_id", processId);
            parameters.Add("state", state);
            parameters.Add("status", processStatus);
            parameters.Add("terminatingUserId", terminatingUserId);


            //Set the retention schedule
            string query = @" UPDATE dbo.PROCESS
                                SET STATE = @state
                                    ,STATUS = @status
                                    ,RETENTION_SCHEDULE_DATETIME = Dateadd(Day, 730, getdate())
                                    ,LAST_CHANGE_ID = @terminatingUserId
                                    , LAST_CHANGE_DATETIME = getdate()
                                WHERE PROCESS_ID = @process_id";

            await cn.ExecuteAsync(query, parameters, transaction);
        }

        private string BuildListClause()
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("SELECT DISTINCT P.[PROCESS_ID] AS ProcessId");
            queryBuilder.AppendLine("FROM RAA.dbo.PROCESS P");
            queryBuilder.AppendLine("WHERE P.[PROCESS_ID] IN @Ids");

            return queryBuilder.ToString();
        }
    }
}
