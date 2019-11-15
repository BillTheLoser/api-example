using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Operations;
using System;

namespace Pnnl.Api.Approvals.Http.Controllers
{
    /// <summary>
    /// Represents a controller that provides a uniform HTTP interface for retrieving people.
    /// </summary>
    [Route("people"), Authorize]
    public class PeopleController : Controller
    {
        /// <summary>
        /// Gets the logger used to write diagnostic information.
        /// </summary>
        /// <value>The<see cref="ILogger{TCategoryName}"/> used to write diagnostic information.</value>
        protected ILogger<PeopleController> _logger { get; }
        private readonly IContextPersonStore _contextPersonStore;

        /// <summary>
        /// Initializes a new instance of the<see cref="PeopleController"/> class.
        /// </summary>
        /// <param name = "logger" > The logger used to write diagnostic information.</param>
        /// <param name = "contextPersonStore" > The store to retrieve the user from.</param>
        /// <exception cref = "ArgumentNullException" > The < paramref name= "logger" /> is < see langword= "null" />.</ exception >
        /// < exception cref= "ArgumentNullException" > The data<paramref name="contextPersonStore"/> is <see langword = "null" />.</ exception >
        public PeopleController(ILogger<PeopleController> logger, IContextPersonStore contextPersonStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contextPersonStore = contextPersonStore ?? throw new ArgumentNullException(nameof(contextPersonStore));
        }

        /// <summary>
        /// Asynchronously retrieves the logged-in user.
        /// </summary>
        /// <returns></returns>
        [HttpGet("self")]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        public IActionResult GetCurrentUserAsync()
        {
            // TODO: Use global exception filters.
            try
            {
                Person user = _contextPersonStore.Get(HttpContext.RequestAborted, HttpContext.Items);

                if (user == null)
                    throw new Exception("Error fetching User Information.");

                return Ok(user);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{HttpContext.TraceIdentifier} Unable to retrieve person. Reason: {exception}");
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
