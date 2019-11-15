using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Operations;
using Pnnl.Data.Paging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Http.Controllers
{
    /// <summary>
    /// Represents a controller that provides a uniform HTTP interface for validating charge code.
    /// </summary>
    [Route("process"), Authorize]
    public class ProcessController : Controller
    {
        /// <summary>
        /// Gets the logger used to write diagnostic information.
        /// </summary>
        /// <value>The <see cref="ILogger{TCategoryName}"/> used to write diagnostic information.</value>
        protected ILogger<ProcessController> _logger { get; }
        private readonly IProcessFacade _processFacade;
        private readonly IContextPersonStore _contextPersonStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessController"/> class.
        /// </summary>
        /// <param name="logger">The logger used to write diagnostic information.</param>
        /// <param name="processFacade">The store to retrieve the user from.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The data <paramref name="processFacade"/> is <see langword="null"/>.</exception>
        public ProcessController(IProcessFacade processFacade, ILogger<ProcessController> logger, IContextPersonStore contextPersonStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processFacade = processFacade ?? throw new ArgumentNullException(nameof(processFacade));
            _contextPersonStore = contextPersonStore ?? throw new ArgumentNullException(nameof(contextPersonStore));
        }

        /// <summary>
        /// Asynchronously retrieves the list of process statuses based on the list provided
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<Process>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdsAsync(IList<int> processIds, [FromQuery]int? offset = null, [FromQuery]int? limit = null)
        {
            try
            {
                var result = await _processFacade.GetAsync(processIds, offset, limit, HttpContext.RequestAborted, HttpContext.Items);

                if (result == null)
                    return NotFound(result);

                return Ok(result);
            }
            catch(ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to retrieve status. Reason: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously instantiates a new routing
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRouting([FromBody] RoutingItem routingItem)
        {
            try
            {
                var result = await _processFacade.CreateRoutingAsync(routingItem, HttpContext.RequestAborted, HttpContext.Items);

                if (result == null)
                    return NotFound(result);

                return Ok(result);
            }
            catch (AggregateException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to create the routing. Reason: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously terminates a <see cref="Process"/> with id <paramref name="processId"/>.
        /// </summary>
        /// <param name="processId">The unique identified of the <see cref="Process"/> to terminate.</param>
        /// <param name="terminateNoStatusing">An optional boolean flag when set to true, terminates the process by no statusing.</param>
        /// <returns>An object of type <see cref="TerminateProcessResponse"/>.</returns>
        [HttpPost, Route("{processId:int}/terminate")]
        [ProducesResponseType(typeof(TerminateProcessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> TerminateProcess(int processId, bool terminateNoStatusing = false)
        {
            try
            {
                Person user = _contextPersonStore.Get(HttpContext.RequestAborted, HttpContext.Items);

                if (user == null)
                    throw new Exception("Error fetching User Information.");

                var terminateProcessResponse = await _processFacade.TerminateProcessAsync(processId, terminateNoStatusing, user, HttpContext.RequestAborted, HttpContext.Items);

                return Ok(terminateProcessResponse);
            }
            catch (InvalidOperationException opEx)
            {
                //https://stackoverflow.com/questions/44597099/asp-net-core-giving-me-code-500-on-forbid
                //return StatusCode(403, opEx.Message);
                return Conflict(opEx);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to terminate the process. Reason: {exception}");
                throw exception;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the list of processes based on the filter provided.
        /// </summary>
        /// <returns>A <see cref="IPagedResult{T}" of type <see cref="Process"/>/></returns>
        [HttpGet, Route("filter")]
        [ProducesResponseType(typeof(PagedResult<Process>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FilterAsync(List<string> actorIds = null,
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
            [FromQuery]int? offset = null, [FromQuery]int? limit = null)
        {
            try
            {
                Person user = _contextPersonStore.Get(HttpContext.RequestAborted, HttpContext.Items);

                if (user == null)
                    throw new Exception("Error fetching User Information.");

                var processFilter = _processFacade.GenerateProcessFilter(offset, limit, actorIds, orginatorIds, beneficaryIds, actionTakenList, activityStateNames, createDateStart, createDateEnd, createDays, lastDateStart, lastDateEnd, lastDays, docTypeNames, processStateNames);

                var result = await _processFacade.SearchAsync(processFilter, user, offset, limit, HttpContext.RequestAborted, HttpContext.Items);

                if (result == null)
                    return NotFound(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentNullException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (ArgumentException exception)
            {
                return NotFound(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to filter the process. Reason: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// If <see langword="true"/>, the method has been directly or indirectly by a user's code;
        /// if <see langword="false"/> the method has been called by the runtime from inside a finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing) { }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}