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
    /// Represents a controller that provides a uniform HTTP interface for activity resource.
    /// </summary>
    [Route("activity"), Authorize]
    public class ActivityController : Controller
    {
        /// <summary>
        /// Gets the logger used to write diagnostic information.
        /// </summary>
        /// <value>The <see cref="ILogger{TCategoryName}"/> used to write diagnostic information.</value>
        protected ILogger<ActivityController> _logger { get; }
        private readonly IActivityFacade _activityFacade;
        private readonly IContextPersonStore _contextPersonStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityController"/> class.
        /// </summary>
        /// <param name="logger">The logger used to write diagnostic information.</param>
        /// <param name="activityFacade">The store to retrieve the user from.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The data <paramref name="activityFacade"/> is <see langword="null"/>.</exception>
        public ActivityController(IActivityFacade activityFacade, ILogger<ActivityController> logger, IContextPersonStore contextPersonStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _activityFacade = activityFacade ?? throw new ArgumentNullException(nameof(activityFacade));
            _contextPersonStore = contextPersonStore ?? throw new ArgumentNullException(nameof(contextPersonStore));
        }

        /// <summary>
        /// Asynchronously retrieves the list of activities based on the list of activitIds provided.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PagedResult<Pnnl.Api.Approvals.Activity>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByIdsAsync(IList<int> activityIds, [FromQuery]int? offset = null, [FromQuery]int? limit = null)
        {
            try
            {
                var result = await _activityFacade.GetAsync(activityIds, offset, limit, HttpContext.RequestAborted, HttpContext.Items);

                if (result == null)
                    return NotFound(result);

                return Ok(result);
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
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to retrieve status. Reason: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously applies and actor action to the activity
        /// </summary>
        /// <returns></returns>
        [HttpPost("")]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApplyActorAction([FromBody] ActorAction actorAction)
        {
            //  We need to get the authorized user and pass that thorugh so we can use that in the security model.
            try
            {
                Person user = _contextPersonStore.Get(HttpContext.RequestAborted, HttpContext.Items);

                if (user == null)
                    throw new Exception("Error fetching User Information.");

                var result = await _activityFacade.ApplyActorActionAsync(actorAction, user, HttpContext.RequestAborted, HttpContext.Items);

                if (result == null)
                    return NotFound(result);

                return Ok(result);
            }
            catch (AggregateException exception)
            {
                _logger.LogInformation($"{HttpContext.TraceIdentifier} AggregateException: {exception}");
                return BadRequest(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogInformation($"{HttpContext.TraceIdentifier} InvalidOperationException: {exception}");
                return BadRequest(exception.Message);
            }
            catch (ArgumentException exception)
            {
                _logger.LogInformation($"{HttpContext.TraceIdentifier} ArgumentException: {exception}");
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to Apply Actor Action. Reason: {exception}");
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
                if (disposing) {}
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}