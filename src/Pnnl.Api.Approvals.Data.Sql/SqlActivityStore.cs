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

namespace Pnnl.Api.Approvals.Data.Sql
{
    /// <summary>
    /// The SQL data store to access the activity information
    /// </summary>
    public class SqlActivityStore : ActivityStoreBase
    {
        private readonly ILogger<SqlActivityStore> _logger;
        private readonly IOptions<SqlActivityStoreOptions> _options;
        private readonly ISqlConnectionFactory _connectionFactory;

        /// <summary>
        /// Constructor with basic parameters for dependency injection.
        /// </summary>
        /// <param name="logger">The logger for capturing logs.</param>
        /// <param name="options">Options for the store</param>
        /// <param name="connectionFactory">The connection factory used to generate connections for the applciation.</param>
        public SqlActivityStore(ILogger<SqlActivityStore> logger, IOptions<SqlActivityStoreOptions> options, ISqlConnectionFactory connectionFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Activity"/>.
        /// </summary>
        /// <param name="activityIds">The list of activity identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected override async Task<PagedResult<Activity>> OnGetAsync(IList<int> activityIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                //int count = await GetActivityCountAsync(activityIds);
                int count = activityIds.Count;

                List<int> subList = activityIds.Skip(offset).Take(limit).ToList<int>();


                var policy = _connectionFactory.PolicySelector(_logger);

                var result = await policy.ExecuteAsync(async () =>
                {
                    using (var connection = _connectionFactory.Create("raa"))
                    {
                        IDictionary<int, Activity> activityList = await GetActivityList(subList, connection);

                        activityList = PopulateActivitiesNotFound(subList, activityList);

                        return new PagedResult<Activity>(offset, limit, count, activityList.Values.ToList());
                    }
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve activity list. Reason: {ex}");
                throw;
            }
        }

        private IDictionary<int, Activity> PopulateActivitiesNotFound(List<int> subList, IDictionary<int, Activity> activityList)
        {
            const string errMsg = "ITEM NOT FOUND";
            foreach (int id in subList)
            {
                if (!activityList.ContainsKey(id))
                {
                    activityList.Add(id, new Activity() { ActivityId = id, ActivityState = errMsg, ActivityStatus = errMsg });
                }
            }

            return activityList;
        }

        private static async Task<IDictionary<int, Activity>> GetActivityList(List<int> subList, SqlConnection cn)
        {
            string sqlStmnt = @"
SELECT A.ACTIVITY_ID AS ActivityId
    ,A.ACTIVITY_NAME AS ActivityName
    ,A.PENDING_DTM AS PendingDateTime
    ,A.LAST_CHANGE_DATETIME AS LastChangeDateTime
    ,a.STATE AS ActivityState
    ,A.STATUS AS ActivityStatus
    ,A.COMMENT AS Comment
    ,AA.HANFORD_ID AS ActedHanfordId
    ,A.ACTED_ACTOR_ID AS ActedActorId
    ,A.IS_GHOST AS IsGhost
    ,A.IS_ADHOC AS IsAdhoc
    ,ACT.ACTOR_ID AS ActorId
    ,ACT.ACTOR_CRITERIA_ID AS ActorCriteriaId
    ,AH.HANFORD_ID AS HanfordId
    ,ACT.ACTOR_TYPE AS ActorType
    ,AD.HANFORD_ID AS DelegatorHanfordId
FROM RAA.dbo.ACTIVITY A 
LEFT OUTER JOIN opwhse.dbo.vw_pub_person AA on A.ACTED_USERID = AA.EMPLID
LEFT OUTER JOIN RAA.dbo.ACTOR ACT on A.ACTIVITY_ID = ACT.ACTIVITY_ID
LEFT OUTER JOIN opwhse.dbo.vw_pub_person AH on ACT.EMPLID = AH.EMPLID
LEFT OUTER JOIN opwhse.dbo.vw_pub_person AD on ACT.DELEGATOR_ID = AD.EMPLID
WHERE A.ACTIVITY_ID in @Ids";
            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(new { Ids = subList });

            IDictionary<int, Activity> activityList = new Dictionary<int, Activity>(subList.Count);

            await cn.QueryAsync<Activity, Actor, Activity>(sqlStmnt
                , (a, act) =>
                {
                    if (!activityList.TryGetValue(a.ActivityId, out Activity activity))
                    {
                        activityList.Add(a.ActivityId, activity = a);
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
                    return activity;
                }
                , parameters
                , splitOn: "ActivityId,ActorId"
                , commandType: CommandType.Text);
            return activityList;

        }
    }
}
