using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data
{
    /// <summary>
    /// The base class for the concrete implementations of the process store.
    /// </summary>
    public abstract class ProcessStoreBase : IProcessStore
    {
        /// <summary>
        /// Base class for interacting with the <see cref="Process"/> item in the approvals system.
        /// </summary>
        protected ProcessStoreBase()
        {
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
        public virtual async Task<PagedResult<Process>> GetAsync(IList<int> processIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processIds == null)
                throw new ArgumentNullException(nameof(processIds));

            if (offset > processIds.Count)
                throw new ArgumentException($"{nameof(offset)} is larger then requested result, will always be empty!");

            if (limit <= 0)
                throw new ArgumentException($"{nameof(limit)} must be a positive integer!");

            return await OnGetAsync(processIds,offset,limit, cancellationToken, context ?? new Dictionary<object, object>());
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
        protected abstract Task<PagedResult<Process>> OnGetAsync(IList<int> processIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously searches the <see cref="Process"/> store based on the filter criteria with security based on the actor is the user
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of ChargeCodeInfo objects.</returns>
        public virtual async Task<PagedResult<Process>> SearchWithActorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processFilter == null)
                throw new ArgumentNullException(nameof(processFilter));

            if (offset < 0)
                throw new ArgumentException($"{nameof(offset)} must be a positive integer!");

            if (limit <= 0)
                throw new ArgumentException($"{nameof(limit)} must be a positive integer!");

            return await OnSearchWithActorAsync(processFilter, offset, limit, cancellationToken, context ?? new Dictionary<object, object>());
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Activity"/>.
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected abstract Task<PagedResult<Process>> OnSearchWithActorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously searches the <see cref="Process"/> store based on the filter criteria with security based on the originator or benificiry is the user
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of ChargeCodeInfo objects.</returns>
        public virtual async Task<PagedResult<Process>> SearchWithOriginatorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processFilter == null)
                throw new ArgumentNullException(nameof(processFilter));

            if (offset < 0)
                throw new ArgumentException($"{nameof(offset)} must be a positive integer!");

            if (limit <= 0)
                throw new ArgumentException($"{nameof(limit)} must be a positive integer!");

            return await OnSearchWithOriginatorAsync(processFilter, offset, limit, cancellationToken, context ?? new Dictionary<object, object>());
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Activity"/>.
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected abstract Task<PagedResult<Process>> OnSearchWithOriginatorAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously searches the <see cref="Process"/> store based on the filter criteria with security based on the permissions of the user
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of ChargeCodeInfo objects.</returns>
        public virtual async Task<PagedResult<Process>> SearchWithUserAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processFilter == null)
                throw new ArgumentNullException(nameof(processFilter));

            if (offset < 0)
                throw new ArgumentException($"{nameof(offset)} must be a positive integer!");

            if (limit <= 0)
                throw new ArgumentException($"{nameof(limit)} must be a positive integer!");

            return await OnSearchWithUserAsync(processFilter, offset, limit, cancellationToken, context ?? new Dictionary<object, object>());
        }

        /// <summary>
        /// Asynchronously retrieves a list of <see cref="Activity"/>.
        /// </summary>
        /// <param name="processFilter">The filter criteria to apply to the result set.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected abstract Task<PagedResult<Process>> OnSearchWithUserAsync(ProcessFilter processFilter, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

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
        public virtual async Task<Process> TerminateAsync( int processId, string terminatingUserId, string processStatus, bool sendNotifications = true, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {

            cancellationToken.ThrowIfCancellationRequested();

            if (processId < 1) throw new ArgumentException(nameof(processId));
            if (string.IsNullOrEmpty(terminatingUserId)) throw new ArgumentNullException(nameof(terminatingUserId));
            if (string.IsNullOrEmpty(processStatus)) throw new ArgumentNullException(nameof(processStatus));

            return await OnTerminateAsync(processId, terminatingUserId, processStatus, sendNotifications, cancellationToken, context ?? new Dictionary<object, object>());
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
        protected abstract Task<Process> OnTerminateAsync(int processId, string terminatingUserId, string processStatus, bool sendNotifications, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
