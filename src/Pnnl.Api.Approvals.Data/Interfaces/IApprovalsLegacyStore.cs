using Pnnl.Api.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    /// <summary>
    /// Describes a repository capable of managing <see cref="RoutingItem"/> resources.
    /// </summary>
    public interface IApprovalsLegacyStore
    {
        /// <summary>
        /// Asynchronously routes an item for approvals
        /// </summary>
        /// <param name="routingItem">The information necessary to instantiate a new routing.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        Task<int?> CreateRoutingAsync(RoutingItem routingItem, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously applies the actor action to the activity
        /// </summary>
        /// <param name="actorAction">The action that is being applied to the activity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        Task<int?> ApplyActorActionAsync(ActorAction actorAction, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
        
        /// <summary>
        /// Asynchronously terminates the <see cref="Process"/> instance based on the unique identifier provided
        /// </summary>
        /// <param name="processId">The unquie identifier of the process to be terminated.</param>
        /// <param name="terminatingUser">The person making the request for the process to be terminated.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The <see cref="Process"/> after the change has been applied.</returns>
        Task<TerminateProcessResponse> TerminateProcessAsync(int processId, Person terminatingUser, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously terminates the <see cref="Process"/> instance based on the unique identifier provided without providing any emails or notifications
        /// </summary>
        /// <param name="processId">The unquie identifier of the process to be terminated.</param>
        /// <param name="terminatingUser">The person making the request for the process to be terminated.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The <see cref="Process"/> after the change has been applied.</returns>
        Task<TerminateProcessResponse> TerminateByProcessNoStatusingAsync(int processId, Person terminatingUser, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
