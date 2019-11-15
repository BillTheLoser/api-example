using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    /// <summary>
    /// A data store to assisting in normalizing a person's identification numbers to their Hanford Id
    /// </summary>
    public interface IPersonIdentificationStore
    {
        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/> or empty.</exception>
        Task<PersonIdentification> GetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/> or empty.</exception>
        Task<PersonIdentification> GetByEmployeeIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/> or empty.</exception>
        Task<PersonIdentification> GetByHanfordIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the resource with the specified <paramref name="networkId"/>.
        /// </summary>
        /// <param name="domain">The network domain of the user to retrieve.</param>
        /// <param name="networkId">The unique identifier of the resource to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the resource with the specified <paramref name="networkId"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="networkId"/> is <see langword="null"/> or empty.</exception>
        Task<PersonIdentification> GetByNetworkIdAsync(string domain, string networkId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
