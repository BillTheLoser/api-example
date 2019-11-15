using Microsoft.Extensions.Caching.Memory;
using Pnnl.Api.Approvals.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data
{
    /// <summary>
    /// The base class for the concrete implementations of the process store.
    /// </summary>
    public abstract class PersonIdentificationStoreBase : IPersonIdentificationStore
    {
        private IMemoryCache _cache;
        //private static readonly int SlidingCacheExpirationTime = 5000;
        private static readonly string CacheName = "PersonIdentificaitonList";

        /// <summary>
        /// Base class for interacting with the <see cref="PersonIdentification"/> item in the approvals system.
        /// </summary>
        protected PersonIdentificationStoreBase(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="id">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        public async Task<PersonIdentification> GetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            try
            {
                // TODO: How to update the cache.
                if (!_cache.TryGetValue(CacheName, out IList<PersonIdentification> list))
                {
                    list = new List<PersonIdentification>();
                }

                PersonIdentification personIdentification = list.FirstOrDefault(i => i.HanfordId == id);

                if(personIdentification == null)
                    personIdentification = list.FirstOrDefault(i => i.EmployeeId == id);

                if(personIdentification == null)
                    personIdentification = list.FirstOrDefault(i => i.NetworkId == id);

                if(personIdentification == null)
                {
                    personIdentification = await OnGetByIdAsync(id, cancellationToken, context ?? new Dictionary<object, object>());
                    list.Add(personIdentification);

                    DateTime expiry = DateTime.Today.AddHours(2).AddDays(1);

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = expiry
                    };

                    _cache.Set(CacheName, list, cacheEntryOptions);
                }

                return personIdentification;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="id">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected abstract Task<PersonIdentification> OnGetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="id">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        public async Task<PersonIdentification> GetByEmployeeIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            try
            {
                // TODO: How to update the cache.
                if (!_cache.TryGetValue(CacheName, out IList<PersonIdentification> list))
                {
                    list = new List<PersonIdentification>();
                }

                PersonIdentification personIdentification = list.FirstOrDefault(i => i.EmployeeId == id);

                if (personIdentification == null)
                {
                    personIdentification = await OnGetByEmployeeIdAsync(id, cancellationToken, context ?? new Dictionary<object, object>());
                    list.Add(personIdentification);

                    DateTime expiry = DateTime.Today.AddHours(2).AddDays(1);

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = expiry
                    };

                    _cache.Set(CacheName, list, cacheEntryOptions);
                }

                return personIdentification;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="id">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected abstract Task<PersonIdentification> OnGetByEmployeeIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="id">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        public async Task<PersonIdentification> GetByHanfordIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            try
            {
                // TODO: How to update the cache.
                if (!_cache.TryGetValue(CacheName, out IList<PersonIdentification> list))
                {
                    list = new List<PersonIdentification>();
                }

                PersonIdentification personIdentification = list.FirstOrDefault(i => i.EmployeeId == id);

                if (personIdentification == null)
                {
                    personIdentification = await OnGetByHanfordIdAsync(id, cancellationToken, context ?? new Dictionary<object, object>());
                    list.Add(personIdentification);

                    DateTime expiry = DateTime.Today.AddHours(2).AddDays(1);

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = expiry
                    };

                    _cache.Set(CacheName, list, cacheEntryOptions);
                }

                return personIdentification;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="id">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected abstract Task<PersonIdentification> OnGetByHanfordIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="networkId">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="domain">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        public async Task<PersonIdentification> GetByNetworkIdAsync(string domain, string networkId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(networkId))
                throw new ArgumentNullException(nameof(networkId));

            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException(nameof(domain));

            try
            {
                // TODO: How to update the cache.
                if (!_cache.TryGetValue(CacheName, out IList<PersonIdentification> list))
                {
                    list = new List<PersonIdentification>();
                }

                PersonIdentification personIdentification = list.FirstOrDefault(i => i.NetworkId == networkId && i.Domain == domain);

                if (personIdentification == null)
                {
                    personIdentification = await OnGetByNetworkIdAsync(domain, networkId, cancellationToken, context ?? new Dictionary<object, object>());
                    list.Add(personIdentification);

                    DateTime expiry = DateTime.Today.AddHours(2).AddDays(1);

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = expiry
                    };

                    _cache.Set(CacheName, list, cacheEntryOptions);
                }

                return personIdentification;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="PersonIdentification"/>.
        /// </summary>
        /// <param name="networkId">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="domain">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> </returns>
        protected abstract Task<PersonIdentification> OnGetByNetworkIdAsync(string domain, string networkId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);
    }
}
