using Domain.DTOs;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IMediator mediator, ILogger<ClientsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all books borrowed by a specific client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <returns>List of books borrowed by the client</returns>
        [HttpGet("{clientId}/books")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByClient(int clientId)
        {
            _logger.LogInformation("Getting books for client ID: {ClientId}", clientId);

            var query = new GetBooksByClientQuery(clientId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}