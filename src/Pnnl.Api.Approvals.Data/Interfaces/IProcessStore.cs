using Pnnl.Data.Paging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    /// <summary>
    /// Describes a repository capable of managing <see cref="Process"/> resources.
    /// </summary>
    public interface IProcessStore
    {
        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="processIds">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The list of <see cref="Process"/> based on the list of unique identifiers provided.</returns>
        Task<PagedResult<Process>> GetAsync(IList<int> processIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously searches the <see cref="Process"/> store based on the filter criteria
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The list of <see cref="Process"/> based on the filter criteria.</returns>
        //Task<PagedResult<Process>> SearchAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);


        /// <summary>
        /// Asynchronously searches the <see cref="Process"/> store based on the filter criteria with security based on the actor is the user
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The list of <see cref="Process"/> based on the filter criteria.</returns>
        Task<PagedResult<Process>> SearchWithActorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously searches the <see cref="Process"/> store based on the filter criteria with security based on the originator or benificiry is the user
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The list of <see cref="Process"/> based on the filter criteria.</returns>
        Task<PagedResult<Process>> SearchWithOriginatorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously searches the <see cref="Process"/> store based on the filter criteria with security based on the permissions of the user
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>The list of <see cref="Process"/> based on the filter criteria.</returns>
        Task<PagedResult<Process>> SearchWithUserAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

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
        Task<Process> TerminateAsync( int processId, string terminatingUserId, string processStatus, bool sendNotifications, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

    }
}
