using Pnnl.Data.Paging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    public interface IProcessNodeStore
    {
        Task<PagedResult<ProcessNodeResult>> GetByIdsAsync(IList<int> processIds, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
