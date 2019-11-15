using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data
{
    public abstract class ProcessNodeStoreBase : IProcessNodeStore
    {
        public virtual async Task<PagedResult<ProcessNodeResult>> GetByIdsAsync(IList<int> processIds, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (limit > 100)
                throw new ArgumentException("Limit must be less that 100!");

            limit = limit ?? 50;
            offset = offset ?? 0;

            if (processIds == null || processIds.Count == 0)
                throw new ArgumentNullException(nameof(processIds));

            cancellationToken.ThrowIfCancellationRequested();

            return await OnGetByIdsAsync(processIds, offset.GetValueOrDefault(), limit.GetValueOrDefault(), cancellationToken, context);
        }

        protected abstract Task<PagedResult<ProcessNodeResult>> OnGetByIdsAsync(IList<int> processIds, int offset, int limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
