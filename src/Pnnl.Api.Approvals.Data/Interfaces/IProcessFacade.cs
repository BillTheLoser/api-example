using Pnnl.Api.Operations;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    /// <summary>
    /// Describes an object capable of managing <see cref="Process"/> and <see cref="ActorAction"/> resources.
    /// </summary>
    public interface IProcessFacade
    {
        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="processIds">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of process objects.</returns>
        Task<PagedResult<Process>> GetAsync(IList<int> processIds, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously routes an item for approvals
        /// </summary>
        /// <param name="routingItem">The information necessary to instantiate a new routing.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        Task<Process> CreateRoutingAsync(RoutingItem routingItem, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously terminates the routing of an instantiated process
        /// </summary>
        /// <param name="processId">The unique identifier of the item that will be terminated.</param>
        /// <param name="terminateNoStatusing">The flag that idicates whether the remote web service will be called and whether or not email will be sent.</param>
        /// <param name="terminatingUser">The person that is perfoming and action (should be a super user on the process).</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        Task<TerminateProcessResponse> TerminateProcessAsync(int processId, bool terminateNoStatusing, Person terminatingUser, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="processFilter">The list of activity identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        Task<PagedResult<Process>> SearchAsync(ProcessFilter processFilter, Person user, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Synchrounously creates a new process filter based on the search filter parameters provided.
        /// </summary>
        /// <returns></returns>
        ProcessFilter GenerateProcessFilter(int? offset,
            int? limit, 
            List<string> actorIds = null,
            List<string> orginatorIds = null,
            List<string> beneficaryIds = null,
            List<ActorActionTaken> actionTakenList = null,
            List<string> activityStateNames = null,
            DateTime? createDateStart = null,
            DateTime? createDateEnd = null,
            int? createDays = null,
            DateTime? lastDateStart = null,
            DateTime? lastDateEnd = null,
            int? lastDays = null,
            List<string> docTypeNames = null,
            List<string> processStateNames = null,
            IDictionary<object, object> context = null);
    }
}
