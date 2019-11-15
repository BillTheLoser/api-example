using Pnnl.Api.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    public interface IContextPersonStore
    {
        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{Person}"/> whose result yields an <see cref="Person"/> that contains the resources.</returns>
        Person Get(CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
