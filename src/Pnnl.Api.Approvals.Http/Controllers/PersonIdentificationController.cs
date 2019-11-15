using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Data.Paging;
using System;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Http.Controllers
{
    /// <summary>
    /// Represents a controller that provides a uniform HTTP interface for validating charge code.
    /// </summary>
    [Route("personidentification"), Authorize]
    public class PersonIdentificationController : Controller
    {
        /// <summary>
        /// Gets the logger used to write diagnostic information.
        /// </summary>
        /// <value>The <see cref="ILogger{TCategoryName}"/> used to write diagnostic information.</value>
        protected ILogger<PersonIdentificationController> _logger { get; }
        private readonly IPersonIdentificationStore _personIdtore;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonIdentificationController"/> class.
        /// </summary>
        /// <param name="logger">The logger used to write diagnostic information.</param>
        /// <param name="processStore">The store to retrieve the user from.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="logger"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The data <paramref name="processStore"/> is <see langword="null"/>.</exception>
        public PersonIdentificationController(IPersonIdentificationStore processStore, ILogger<PersonIdentificationController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _personIdtore = processStore ?? throw new ArgumentNullException(nameof(processStore));
        }

        /// <summary>
        /// Asynchronously retrieves the person from the employeeId
        /// </summary>
        /// <returns></returns>
        [HttpGet("employeeid")]
        [ProducesResponseType(typeof(PagedResult<PersonIdentification>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmployeeIdAsync(string id)
        {
            try
            {
                var result = await _personIdtore.GetByEmployeeIdAsync(id, HttpContext.RequestAborted, HttpContext.Items);

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
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to retrieve status. Reason: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the person from hanfordId.
        /// </summary>
        /// <returns></returns>
        [HttpGet("hanfordId")]
        [ProducesResponseType(typeof(PagedResult<PersonIdentification>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByHanfordIdAsync(string id)
        {
            try
            {

                var result = await _personIdtore.GetByHanfordIdAsync(id, HttpContext.RequestAborted, HttpContext.Items);

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
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to retrieve status. Reason: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the person from the networkId.
        /// </summary>
        /// <returns></returns>
        [HttpGet("networkid")]
        [ProducesResponseType(typeof(PagedResult<PersonIdentification>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNetworkIdAsync(string domain, string id)
        {
            try
            {
                var result = await _personIdtore.GetByNetworkIdAsync(domain, id, HttpContext.RequestAborted, HttpContext.Items);

                if (result == null)
                    return NotFound(result);

                return Ok(result);
            }
            catch (ArgumentException exception)
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
        /// Asynchronously retrieves the person from Id.
        /// </summary>
        /// <returns></returns>
        [HttpGet("id")]
        [ProducesResponseType(typeof(PagedResult<PersonIdentification>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                var result = await _personIdtore.GetByIdAsync(id, HttpContext.RequestAborted, HttpContext.Items);

                if (result == null)
                    return NotFound(result);

                return Ok(result);
            }
            catch (ArgumentException exception)
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