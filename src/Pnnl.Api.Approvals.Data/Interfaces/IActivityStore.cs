using Pnnl.Data.Paging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    /// <summary>
    /// Describes a repository capable of managing <see cref="Activity"/> resources.
    /// </summary>
    public interface IActivityStore
    {
        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="activityIds">The list of activity identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of ChargeCodeInfo objects.</returns>
        Task<PagedResult<Activity>> GetAsync(IList<int> activityIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
