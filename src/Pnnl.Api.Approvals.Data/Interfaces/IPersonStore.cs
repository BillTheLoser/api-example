using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pnnl.Data.Paging;
using Pnnl.Api.Operations;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    /// <summary>
    /// Describes a repository capable of retrieving information about a person.
    /// </summary>
    public interface IPersonStore : IDisposable
    {
        /// <summary>
        /// Asynchronously retrieves resources.
        /// </summary>
        /// <param name="limit">The maximum number of resources to retrieve.</param>
        /// <param name="offset">The page index to begin resource retrieval from.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields an <see cref="IPagedResult"/> that contains the resources that were retrieved.</returns>
        Task<IPagedResult<Person>> GetAsync(int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<string, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/> or empty.</exception>
        Task<Person> GetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<string, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="domain">The network domain of the user to retrieve.</param>
        /// <param name="id">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/> or empty.</exception>
        Task<Person> GetByNetworkIdAsync(string domain, string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<string, object> context = null);
    }
}
