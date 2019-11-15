using Microsoft.Extensions.Caching.Memory;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data
{
    public abstract class SecurityStoreBase : ISecurityStore
    {
        private IMemoryCache _cache;
        //private static int SlidingCacheExpirationTime = 5000;
        private static string CacheName = "AuthorizedAcountList";

        /// <summary>
        /// Base class for interacting with the <see cref="SecurityStoreBase"/> item in the approvals system.
        /// </summary>
        protected SecurityStoreBase(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<IList<string>> GetAuthorizedAccountsAsync(Process process, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (process == null)    
                throw new ArgumentNullException(nameof(process));

            cancellationToken.ThrowIfCancellationRequested();

            return await OnGetAuthorizedAccountsAsync(process, cancellationToken, context ?? new Dictionary<object, object>());
        }

        protected abstract Task<IList<string>> OnGetAuthorizedAccountsAsync(Process process, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        public async Task<bool> IsSuperUserAsync(int processId, string userId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User id - HanfordId/EmployeeId is null or empty.");

            cancellationToken.ThrowIfCancellationRequested();

            return await OnIsSuperUserAsync(processId, userId, cancellationToken, context ?? new Dictionary<object, object>());

        }
        protected abstract Task<bool> OnIsSuperUserAsync(int processId, string userId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously returns whether or not the user is an approvals admin
        /// </summary>
        /// <param name="userId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields whether the user is an Approvals admin.</returns>
        public async Task<bool> IsApprovalsAdminAsync( string userId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User id - HanfordId/EmployeeId is null or empty.");

            cancellationToken.ThrowIfCancellationRequested();

            return await OnIsApprovalsAdminAsync(userId, cancellationToken, context ?? new Dictionary<object, object>());

        }

        /// <summary>
        /// Asynchronously returns whether or not the user is an approvals admin
        /// </summary>
        /// <param name="userId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields whether the user is an Approvals admin.</returns>
        protected abstract Task<bool> OnIsApprovalsAdminAsync(string userId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the list of document types that the specified user has the ability to view
        /// </summary>
        /// <param name="user">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        public async Task<IList<string>> GetReadDocumentTypesAsync(Person user, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return await OnGetReadDocumentTypesAsync(user, cancellationToken, context ?? new Dictionary<object, object>());
        }

        /// <summary>
        /// Asynchronously retrieves the list of document types that the specified user has the ability to view
        /// </summary>
        /// <param name="user">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        protected abstract Task<IList<string>> OnGetReadDocumentTypesAsync(Person user, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously returns whether or not the network account provided is authorized to make changes to processes of that type.
        /// </summary>
        /// <param name="processDefinitionId">The unique identify of the process definition.</param>
        /// <param name="networkId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        public async Task<bool> IsAuthorizedChangeAccountAsync(int processDefinitionId, string networkId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(networkId)) throw new ArgumentNullException(nameof(networkId));

            try
            {
                IList<string> authAccounts = null;
                IDictionary<int, IList<string>> list;

                // TODO: How to update the cache.
                if (!_cache.TryGetValue(CacheName, out list))
                {
                    list = new Dictionary<int, IList<string>>();
                }

                if(list.ContainsKey(processDefinitionId))
                    authAccounts = list[processDefinitionId];

                if (authAccounts == null)
                {
                    authAccounts = await OnGetAuthorizedChangeAccountsAsync(processDefinitionId, cancellationToken, context ?? new Dictionary<object, object>());
                    list.Add(processDefinitionId, authAccounts);

                    DateTime expiry = DateTime.Today.AddHours(2).AddDays(1);
                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();
                    cacheEntryOptions.AbsoluteExpiration = expiry;

                    _cache.Set(CacheName, list, cacheEntryOptions);
                }

                return authAccounts.Contains(networkId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Asynchronously returns a list of accounts that are authorized to make changes for that process definition   
        /// </summary>
        /// <param name="networkId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        protected abstract Task<IList<string>> OnGetAuthorizedChangeAccountsAsync(int processDefinitionId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);



    }
}
