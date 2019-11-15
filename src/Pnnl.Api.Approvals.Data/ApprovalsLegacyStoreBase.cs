using Pnnl.Api.Operations;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Data
{
    /// <summary>
    /// The base class for the concrete implementations of the <see cref="IApprovalsLegacyStore"/>
    /// </summary>
    public abstract class ApprovalsLegacyStoreBase: IApprovalsLegacyStore
    {
        /// <summary>
        /// Asynchronously routes an item for approvals
        /// </summary>
        /// <param name="routingItem">The information necessary to instantiate a new routing.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        public virtual async Task<int?> CreateRoutingAsync(RoutingItem routingItem, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {

            cancellationToken.ThrowIfCancellationRequested();

            if (routingItem == null) throw new ArgumentNullException(nameof(routingItem));

            ValidationContext validationContext = new ValidationContext(routingItem);
            List<ValidationResult> errors = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(routingItem,validationContext,errors);

            if(!isValid)
            {
                throw new AggregateException(errors.Select(e => new ValidationException(e.ErrorMessage)));
            }

            return await OnCreateRoutingAsync(routingItem, cancellationToken, context ?? new Dictionary<object, object>());
        }

        /// <summary>
        /// Asynchronously routes an item for approvals
        /// </summary>
        /// <param name="routingItem">The information necessary to instantiate a new routing.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        protected abstract Task<int?> OnCreateRoutingAsync(RoutingItem routingItem, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        /// <summary>
        /// Asynchronously applies the actor action to the activity
        /// </summary>
        /// <param name="actorAction">The action that is being applied to the activity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        public async Task<int?> ApplyActorActionAsync(ActorAction actorAction, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {

            cancellationToken.ThrowIfCancellationRequested();

            if (actorAction == null) throw new ArgumentNullException(nameof(actorAction));

            ValidationContext validationContext = new ValidationContext(actorAction);
            List<ValidationResult> errors = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(actorAction, validationContext,errors);

            if(!isValid)
            {
                throw new AggregateException(errors.Select(e => new ValidationException(e.ErrorMessage)));
            }

            return await OnApplyActorActionAsync(actorAction, cancellationToken, context ?? new Dictionary<object, object>());
        }

        /// <summary>
        /// Asynchronously applies the actor action to the activity
        /// </summary>
        /// <param name="actorAction">The action that is being applied to the activity.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of activity objects.</returns>
        protected abstract Task<int?> OnApplyActorActionAsync(ActorAction actorAction, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null);

        public async Task<TerminateProcessResponse> TerminateProcessAsync(int processId, Person terminatingUser, CancellationToken cancellationToken, IDictionary<object, object> context)
        {
            if(string.IsNullOrEmpty(terminatingUser?.Network.Domain))
                throw new ArgumentNullException("Error fetching the requesting user's domain.");
            else if(string.IsNullOrEmpty(terminatingUser?.Network.Username))
                throw new ArgumentNullException("Error fetching the requesting user's username.");

            cancellationToken.ThrowIfCancellationRequested();

            return await OnTerminateProcessAsync(processId, terminatingUser, cancellationToken, context);
        }

        protected abstract Task<TerminateProcessResponse> OnTerminateProcessAsync(int processId, Person terminatingUser, CancellationToken cancellationToken, IDictionary<object, object> context = null);

        public async Task<TerminateProcessResponse> TerminateByProcessNoStatusingAsync(int processId, Person terminatingUser, CancellationToken cancellationToken, IDictionary<object, object> context)
        {
            if (string.IsNullOrEmpty(terminatingUser?.Network.Domain))
                throw new ArgumentNullException("Error fetching the requesting user's domain.");
            else if (string.IsNullOrEmpty(terminatingUser?.Network.Username))
                throw new ArgumentNullException("Error fetching the requesting user's username.");

            cancellationToken.ThrowIfCancellationRequested();

            return await OnTerminateByProcessNoStatusingAsync(processId, terminatingUser, cancellationToken, context);
        }

        protected abstract Task<TerminateProcessResponse> OnTerminateByProcessNoStatusingAsync(int processId, Person terminatingUser, CancellationToken cancellationToken, IDictionary<object, object> context = null);
    }
}
