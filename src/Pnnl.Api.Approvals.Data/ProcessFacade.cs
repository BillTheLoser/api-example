using Microsoft.Extensions.Logging;
using Pnnl.Api.Approvals.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pnnl.Data.Paging;
using System.Threading;
using Pnnl.Api.Operations;

namespace Pnnl.Api.Approvals.Data
{
    /// <summary>
    /// The manages the logic around the process routes
    /// </summary>
    public class ProcessFacade:IProcessFacade
    {
        private readonly IProcessStore _processStore;
        private readonly ILogger<ProcessFacade> _logger;
        private readonly IApprovalsLegacyStore _approvalsLegacyStore;
        private readonly ISecurityStore _securityStore;

        /// <summary>
        /// Describes an object capable of managing <see cref="Process"/> and <see cref="ActorAction"/> resources.
        /// </summary>
        public ProcessFacade(ILogger<ProcessFacade> logger, IProcessStore processStore, IApprovalsLegacyStore approvalsLegacyStore, ISecurityStore securityStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processStore = processStore ?? throw new ArgumentNullException(nameof(processStore));
            _approvalsLegacyStore = approvalsLegacyStore ?? throw new ArgumentNullException(nameof(approvalsLegacyStore));
            _securityStore = securityStore ?? throw new ArgumentNullException(nameof(securityStore));
        }

        /// <summary>
        /// Asynchronously routes an item for approvals
        /// </summary>
        /// <param name="routingItem">The information necessary to instantiate a new routing.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        public async Task<Process> CreateRoutingAsync(RoutingItem routingItem, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            try
            {
                int? id = await _approvalsLegacyStore.CreateRoutingAsync(routingItem, cancellationToken, context);

                IList<int> list = new List<int>
                {
                    id.GetValueOrDefault()
                };

                var result = await _processStore.GetAsync(list,0, 10, cancellationToken, context);

                return result.First();

            }
            catch (Exception exception)
            {
                _logger.LogError($"Unable to process routing request: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="processIds">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of process objects.</returns>
        public async Task<PagedResult<Process>> GetAsync(IList<int> processIds, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (limit > 100)
                throw new ArgumentException("Limit must be less that 100!");

            limit = limit ?? 50;
            offset = offset ?? 0;

            return await _processStore.GetAsync(processIds, offset.GetValueOrDefault(), limit.GetValueOrDefault(), cancellationToken, context);
        }

        /// <summary>
        /// Asynchronously terminates the routing of an instantiated process
        /// </summary>
        /// <param name="processId">The unique identifier of the item that will be terminated.</param>
        /// <param name="terminateNoStatusing">The flag that idicates whether the remote web service will be called and whether or not email will be sent.</param>
        /// <param name="terminatingUser">The person that is perfoming and action (should be a super user on the process).</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>An integer that contains the new process id.</returns>
        public async Task<TerminateProcessResponse> TerminateProcessAsync(int processId, bool terminateNoStatusing, Person terminatingUser, CancellationToken cancellationToken, IDictionary<object, object> context)
        {
            if (terminatingUser == null)
                throw new ArgumentNullException(nameof(terminatingUser));

            try
            {
                Process process = (await _processStore.GetAsync(new List<int> { processId, }, 0, 10)).FirstOrDefault();

                if (process.ProcessState == "TERMINATED" || process.ProcessState == "APPROVED")
                    throw new InvalidOperationException($"Invalid request to terminate Process {process.ProcessId} that is already in {process.ProcessState} state!");

                //IList<string> approvedChangeAccounts = await _securityStore.GetAuthorizedAccountsAsync(process, cancellationToken, context);

                // Verify if the requested account (user or msa account) is authorized to perform this action.
                string userId = terminatingUser?.Network?.Username;
                bool isAuthAccount = false;

                // If the network id is in the authorized change accounts we are done, nothing else matters
                if (userId != null)
                    isAuthAccount = await _securityStore.IsAuthorizedChangeAccountAsync(process.ProcessDefinitionId, userId, cancellationToken, context);

                // if they are not an authorized change account they need to be a super user
                bool isSuperUser = false;
                bool isApprovalsAdmin = false;
                if(!isAuthAccount)
                {
                    if (string.IsNullOrEmpty(userId) || (!string.IsNullOrEmpty(userId) && !isAuthAccount))
                        userId = terminatingUser.Id ?? terminatingUser.EmployeeId;

                    isSuperUser = await _securityStore.IsSuperUserAsync(processId, userId, cancellationToken, context);

                    if(!isSuperUser)
                        isApprovalsAdmin = await _securityStore.IsApprovalsAdminAsync( userId, cancellationToken, context);
                }

                if (isSuperUser || isAuthAccount || isApprovalsAdmin)
                {
                    TerminateProcessResponse terminateProcessResponse = new TerminateProcessResponse();

                    string status = $"{process.DocumentTypeName}'- {process.DocumentId}': TERMINATED by: {userId}";

                    process = await _processStore.TerminateAsync(processId, userId, status, !terminateNoStatusing);
                    if (process.ProcessState != ProcessState.Terminated.Name)
                        throw new InvalidOperationException($"Unable to terminate Process Instance {processId} by user {userId}");

                    terminateProcessResponse.State = process?.ProcessState;
                    terminateProcessResponse.Status = process?.ProcessStatus;
                    terminateProcessResponse.ProcessId = (int)process?.ProcessId;

                    return terminateProcessResponse;
                }
                else
                {
                    throw new UnauthorizedAccessException($"You do not have sufficient privileges to terminate the process with id {processId}");
                }

            }
            catch (Exception exception)
            {
                _logger.LogError($"Unable to terminate process. Reason: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the currently available resources.
        /// </summary>
        /// <param name="processFilter">The list of process identification numbers that we are going to retrieve.</param>
        /// <param name="user">The user that is making the request for information.</param>
        /// <param name="limit">Maximun number of items that we are going to return per page.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <param name="context">The optional execution context that applies to this operation.</param>
        /// <returns>A dictionary of process objects.</returns>
        public async Task<PagedResult<Process>> SearchAsync(ProcessFilter processFilter, Person user, int? offset, int? limit, CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (limit > 100)
                throw new ArgumentException("Limit must be less that 100!");

            limit = limit ?? 50;
            offset = offset ?? 0;

            if (processFilter.ProcessStateList == null || processFilter.ProcessStateList.Count == 0)
            {
                processFilter.ProcessStateList = new List<ProcessState>
        {
                    ProcessState.Approved,
                    ProcessState.NotYetPending,
                    ProcessState.Pending,
                    ProcessState.Terminated
                };
            }

            if (processFilter.ActorIdList != null && processFilter.ActorIdList.Count == 1 && processFilter.ActorIdList[0] == user.Id)
            {
                // since we are seaching on actor, we can improve performance by filtering out the nulls in the activity states
                // because they won't have a corresponding actor record. so if the list is empty we can all the other states
                if (processFilter.ActivityStateList == null)
                {
                    processFilter.ActivityStateList = new List<ActivityState>();
                }

                if (processFilter.ActivityStateList.Count == 0)
                {
                    processFilter.ActivityStateList.Add(ActivityState.Complete);
                    processFilter.ActivityStateList.Add(ActivityState.Escalated);
                    processFilter.ActivityStateList.Add(ActivityState.Pending);
                }

                _logger.LogInformation($"Pnnl.Api.Approvals: User {user.Id} is executing search for actor for {processFilter.ToString()}.");
                return await _processStore.SearchWithActorAsync(processFilter, offset.GetValueOrDefault(), limit.GetValueOrDefault(), cancellationToken, context);
            }
            else if (processFilter.OriginatorIdList != null && processFilter.OriginatorIdList.Count == 1 && processFilter.OriginatorIdList[0] == user.Id)
            {
                _logger.LogInformation($"Pnnl.Api.Approvals: User {user.Id} is executing search for originator for {processFilter.ToString()}.");
                return await _processStore.SearchWithOriginatorAsync(processFilter, offset.GetValueOrDefault(), limit.GetValueOrDefault(), cancellationToken, context);
            }
            else if (processFilter.BeneficiaryIdList != null && processFilter.BeneficiaryIdList.Count == 1 && processFilter.BeneficiaryIdList[0] == user.Id)
            {
                _logger.LogInformation($"Pnnl.Api.Approvals: User {user.Id} is executing search for beneficiary for {processFilter.ToString()}.");
                return await _processStore.SearchWithOriginatorAsync(processFilter, offset.GetValueOrDefault(), limit.GetValueOrDefault(), cancellationToken, context);
            }
            else
            {
                // get read list
                var readTypeList = await _securityStore.GetReadDocumentTypesAsync(user);

                if (processFilter.DocumentTypeList != null && processFilter.DocumentTypeList.Count > 0)
                {
                    // intersect with searched doctypes and update filter object
                    var accessList = processFilter.DocumentTypeList.Intersect(readTypeList);
                    processFilter.DocumentTypeList = accessList.ToList<string>();
                }
                else
                {
                    processFilter.DocumentTypeList = (List<string>)readTypeList;
                }

                _logger.LogInformation($"Pnnl.Api.Approvals: User {user.Id} is executing search by user for {processFilter.ToString()}.");
                return await _processStore.SearchWithUserAsync(processFilter, offset.GetValueOrDefault(), limit.GetValueOrDefault(), cancellationToken, context);
            }
        }

        public ProcessFilter GenerateProcessFilter(int? offset,
            int? limit,
            List<string> actorIds = null,
            List<string> orginatorIds = null,
            List<string> beneficaryIds = null,
            List<ActorActionTaken> actionTakenList = null,
            List<string> activityStateNames = null,
            DateTime? createDateStart = null,
            DateTime? createDateEnd = null,
            int? createDays = null,
            DateTime? lastDateStart = null,
            DateTime? lastDateEnd = null,
            int? lastDays = null,
            List<string> docTypeNames = null,
            List<string> processStateNames = null,
            IDictionary<object, object> context = null)
        {
            try
            {
                ProcessFilter processFilter = new ProcessFilter();

                if (actorIds != null && actorIds.Count > 0)
                {
                    processFilter.ActorIdList = actorIds;
                }

                if (orginatorIds != null && orginatorIds.Count > 0)
                {
                    processFilter.OriginatorIdList = orginatorIds;
                }

                if (beneficaryIds != null && beneficaryIds.Count > 0)
                {
                    processFilter.BeneficiaryIdList = beneficaryIds;
                }

                if (actionTakenList != null && actionTakenList.Count > 0)
                {
                    processFilter.ActionTakenList = actionTakenList;
                }

                if (activityStateNames != null && activityStateNames.Count > 0)
                {
                    processFilter.ActivityStateList = new List<ActivityState>();

                    foreach (string status in activityStateNames)
                    {
                        // don't like this but ...
                        if (status.ToUpper() == ActivityState.Complete.Name.ToUpper())
                            processFilter.ActivityStateList.Add(ActivityState.Complete);
                        else if (status.ToUpper() == ActivityState.Pending.Name.ToUpper())
                            processFilter.ActivityStateList.Add(ActivityState.Pending);
                        else if (status.ToUpper() == ActivityState.Escalated.Name.ToUpper())
                            processFilter.ActivityStateList.Add(ActivityState.Escalated);
                    }
                }

                if (processStateNames != null && processStateNames.Count > 0)
                {
                    processFilter.ProcessStateList = new List<ProcessState>();

                    foreach (string state in processStateNames)
                    {
                        if (state.ToUpper() == ProcessState.Approved.Name.ToUpper())
                            processFilter.ProcessStateList.Add(ProcessState.Approved);
                        else if (state.ToUpper() == ProcessState.Pending.Name.ToUpper())
                            processFilter.ProcessStateList.Add(ProcessState.Pending);
                        else if (state.ToUpper() == ProcessState.Terminated.Name.ToUpper())
                            processFilter.ProcessStateList.Add(ProcessState.Terminated);
                        else if (state.ToUpper() == ProcessState.NotYetPending.Name.ToUpper())
                            processFilter.ProcessStateList.Add(ProcessState.NotYetPending);
                    }
                }

                if (createDateStart != null && createDateEnd != null)
                {
                    processFilter.CreateDateRange = new DateRange(createDateStart.GetValueOrDefault(), createDateEnd.GetValueOrDefault());
                }
                else
                {
                    if (createDays != null)
                    {
                        processFilter.CreateDateRange = new DateRange(createDays.GetValueOrDefault());
                    }
                }

                if (lastDateStart != null && lastDateEnd != null)
                {
                    processFilter.LastChangeDateRange = new DateRange(lastDateStart.GetValueOrDefault(), lastDateEnd.GetValueOrDefault());
                }
                else
                {
                    if (lastDays != null)
                    {
                        processFilter.LastChangeDateRange = new DateRange(lastDays.GetValueOrDefault());
                    }
                }

                if (docTypeNames != null && docTypeNames.Count > 0)
                {
                    processFilter.DocumentTypeList = docTypeNames;
                }

                return processFilter;
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Error generating the process filter. Reason: {exception}");
                throw;
            }
        }
    }
}
