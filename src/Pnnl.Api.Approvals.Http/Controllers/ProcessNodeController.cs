using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pnnl.Api.Approvals.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pnnl.Api.Approvals.Http.Controllers
{
    [Route("process"), Authorize]
    public class ProcessNodeController : Controller
    {
        private readonly ILogger<ProcessNodeController> _logger;
        private readonly IProcessNodeStore _store;

        public ProcessNodeController(ILogger<ProcessNodeController> logger, IProcessNodeStore store)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>
        /// Asynchronously retrieves the process node metadata for the given <paramref name="processIds"/>.
        /// </summary>
        /// <param name="processIds">A <see cref="IList{T}"/> of type int which uniquely identifies the process id.</param>
        /// <param name="offset">Number of pages that will be skipped.</param>
        /// <param name="cancellationToken">The optional token to monitor for cancellation requests.</param>
        /// <returns></returns>
        [HttpGet, Route("metadata")]
        [ProducesResponseType(typeof(IEnumerable<ProcessNodeResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdsAsync([FromQuery] IList<int> processIds, [FromQuery] int? offset = null, [FromQuery]int? limit = null)
        {
            try
            {
               var processNodes = await _store.GetByIdsAsync(processIds, offset, limit, HttpContext.RequestAborted);

                return Ok(processNodes);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while retrieving process nodes. Exception : {ex}");
                throw ex;
            }
        }
    }
}
