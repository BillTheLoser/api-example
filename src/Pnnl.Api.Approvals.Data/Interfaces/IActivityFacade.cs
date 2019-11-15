using Pnnl.Api.Operations;
using Pnnl.Data.Paging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    /// <summary>
    /// Describes an object capable of managing <see cref="Activity"/> and <see cref="ActorAction"/> resources.
    /// </summary>
    public interface IActivityFacade
    {
        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="activityIds">The list of activity identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        Task<PagedResult<Activity>> GetAsync(IList<int> activityIds, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously applies the actor action to the activity
        /// </summary>
        /// <param name="actorAction">The action that is being applied to the activity.</param>
        /// <param name="actingAccount">The account making the call to apply the action.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        Task<Process> ApplyActorActionAsync(ActorAction actorAction, Person actingAccount, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
