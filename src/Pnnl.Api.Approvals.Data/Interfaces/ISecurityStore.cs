using Pnnl.Api.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data.Interfaces
{
    public interface ISecurityStore
    {
        /// <summary>
        /// Asynchronously returns whether or not the user is a super user on the provided process
        /// </summary>
        /// <param name="processId">The unique identify of the process.</param>
        /// <param name="userId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        Task<bool> IsSuperUserAsync(int processId, string userId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously returns whether or not the network account provided is authorized to make changes to processes of that type.
        /// </summary>
        /// <param name="processDefinitionId">The unique identify of the process definition.</param>
        /// <param name="networkId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        Task<bool> IsAuthorizedChangeAccountAsync(int processDefinitionId, string networkId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        Task< IList<string>> GetAuthorizedAccountsAsync(Process process, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously retrieves the list of document types that the specified user has the ability to view
        /// </summary>
        /// <param name="user">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields the list of the specifies document types.</returns>
        Task<IList<string>> GetReadDocumentTypesAsync(Person user, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously returns whether or not the user is an approvals admin
        /// </summary>
        /// <param name="userId">The <see cref="Person"/> object of the user whose permissions we are checking.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> whose result yields whether the user is an Approvals admin.</returns>
        Task<bool> IsApprovalsAdminAsync( string userId, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);


    }
}
