using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data
{
    /// <summary>
    /// The base class for the concrete implementations of the activity store.
    /// </summary>
    public abstract class ActivityStoreBase : IActivityStore
    {
        /// <summary>
        /// Base class for interacting with the <see cref="Activity"/> item in the approvals system.
        /// </summary>
        protected ActivityStoreBase()
        {
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
        public virtual async Task<PagedResult<Activity>> GetAsync(IList<int> activityIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (activityIds == null) throw new ArgumentNullException(nameof(activityIds));

            if (offset > activityIds.Count) throw new ArgumentException($"{nameof(offset)} is larger then requested result, will always be empty!");
            if (limit <= 0) throw new ArgumentException($"{nameof(limit)} must be a positive integer!");

            return await OnGetAsync(activityIds,offset,limit, cancellationToken, context ?? new Dictionary<object, object>());
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
        protected abstract Task<PagedResult<Activity>> OnGetAsync(IList<int> activityIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
