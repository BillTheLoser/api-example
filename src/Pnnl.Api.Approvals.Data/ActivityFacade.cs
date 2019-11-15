using Pnnl.Api.Operations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pnnl.Api.Approvals.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pnnl.Data.Paging;
using System.Threading;

namespace Pnnl.Api.Approvals.Data
{
    /// <summary>
    /// The manages the logic around the activity routes
    /// </summary>
    public class ActivityFacade:IActivityFacade
    {
        private readonly IProcessStore _processStore;
        private readonly IActivityStore _activityStore;
        private readonly ILogger<ActivityFacade> _logger;
        private readonly IApprovalsLegacyStore _approvalsLegacyStore;
        private readonly ISecurityStore _securityStore;
        private readonly IPersonIdentificationStore _personIdentificationStore;

        /// <summary>
        /// Describes an object capable of managing <see cref="Activity"/> and <see cref="ActorAction"/> resources.
        /// </summary>
        public ActivityFacade(ILogger<ActivityFacade> logger, IActivityStore activityStore,IProcessStore processStore, IApprovalsLegacyStore approvalsLegacyStore, IPersonIdentificationStore personIdentificationStore,  ISecurityStore securityStore)
        {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processStore = processStore ?? throw new ArgumentNullException(nameof(processStore));
            _activityStore = activityStore ?? throw new ArgumentNullException(nameof(activityStore));
            _approvalsLegacyStore = approvalsLegacyStore ?? throw new ArgumentNullException(nameof(approvalsLegacyStore));
            _personIdentificationStore = personIdentificationStore ?? throw new ArgumentNullException(nameof(personIdentificationStore));
            _securityStore = securityStore ?? throw new ArgumentNullException(nameof(securityStore));
        }

        /// <summary>
        /// Asynchronously applies the actor action to the activity
        /// </summary>
        /// <param name="actorAction">The action that is being applied to the activity.</param>
        /// <param name="actingUser">The account making the call to apply the action.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        public async Task<Process> ApplyActorActionAsync(ActorAction actorAction, Person actingUser, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                Process process = await GetProcess(actorAction.ProcessId);

                if(process.ProcessState != "PENDING")
                    throw new InvalidOperationException("process state is invalid for changes");

                Activity activity = process.Activities[actorAction.ActivityId];

                if(activity.ActivityState == "COMPLETE" || activity.ActivityState == "NULL")
                    throw new InvalidOperationException("activity state is invalid for changes");

                bool hasRights = await AccountHasChangeRights(actorAction, process, actingUser, cancellationToken, context);

                if(!hasRights)
                    throw new InvalidOperationException("insufficent account permissions");

                if(!IsValidActor(actorAction.ActorHanfordId, activity))
                    throw new InvalidOperationException("Invalid actor!");

                int? id = await _approvalsLegacyStore.ApplyActorActionAsync(actorAction);

                IList<int> list = new List<int>
                {
                    actorAction.ProcessId
                };

                var result = await _processStore.GetAsync(list, 0, 10, cancellationToken, context);

                return result.First();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Unable to process routing request: {exception}");
                throw;
            }
        }

        private bool IsValidActor(string actorHanfordId, Activity activity)
        {
            bool isValid = false; //note this should be false,  but we need to hook up to directory services first.

            foreach (var item in activity.Actors)
            {
                if (item.Value.ActorHanfordId == actorHanfordId)
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

        private async Task<Process> GetProcess(int processId)
        {
            IList<int> list = new List<int>
            {
                processId
            };

            var result = await _processStore.GetAsync(list, 0, 10);
            Process process = result.First();
            return process;
        }

        private async Task<bool> AccountHasChangeRights(ActorAction actorAction,Process process, Person actingUser, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            bool isValid = false; //note this should be false,  but we need to hock up to directory services first.

            if(actingUser.Id == actorAction.ActorHanfordId)
            {
                Activity activity = process.Activities[actorAction.ActivityId];

                foreach (var item in activity.Actors)
                {
                    if (item.Value.ActorHanfordId == actorAction.ActorHanfordId)
                    {
                        isValid = true;
                        return isValid;
                    }
                }
            }
            else
            {
                isValid = await _securityStore.IsAuthorizedChangeAccountAsync(process.ProcessDefinitionId, actingUser.Network.Username, cancellationToken, context);
                return isValid;
            }

            if(!isValid)
                _logger.LogInformation($"Approvals User: {actingUser.Network.Username} does not have rights to act on Activity {actorAction.ActivityId} for {actorAction.ActorHanfordId}!");

            return isValid;
        }
        
        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="activityIds">The list of activity identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        public async Task<PagedResult<Activity>> GetAsync(IList<int> activityIds, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (limit > 100)
                throw new ArgumentException("Limit must be less that 100!");

            limit = limit ?? 50;
            offset = offset ?? 0;

            return await _activityStore.GetAsync(activityIds, offset.GetValueOrDefault(), limit.GetValueOrDefault(), cancellationToken, context);
        }
    }
}
