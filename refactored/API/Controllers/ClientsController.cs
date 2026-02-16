using Domain.Commands;
using Domain.DTOs;
using Domain.Models;
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
        /// Gets all clients
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Client>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll()
        {
            _logger.LogInformation("Getting all clients");
            var query = new GetListGenericQuery<Client>();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific client by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Client>> GetById(int id)
        {
            _logger.LogInformation("Getting client with ID: {ClientId}", id);
            var query = new GetGenericQuery<Client>(id);
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Client with ID {id} not found.");
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Creates a new client
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Client), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Client>> Create([FromBody] Client client)
        {
            _logger.LogInformation("Creating new client: {FirstName} {LastName}", client.FirstName, client.LastName);
            var command = new AddGenericCommand<Client>(client);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing client
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Client>> Update(int id, [FromBody] Client client)
        {
            _logger.LogInformation("Updating client with ID: {ClientId}", id);
            client.Id = id;
            var command = new UpdateGenericCommand<Client>(client);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a client
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting client with ID: {ClientId}", id);
            var command = new DeleteGenericCommand<Client>(id);
            var result = await _mediator.Send(command);
            
            if (!result)
            {
                return NotFound($"Client with ID {id} not found.");
            }
            
            return NoContent();
        }

        /// <summary>
        /// Gets active clients only
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<Client>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Client>>> GetActive()
        {
            _logger.LogInformation("Getting all active clients");
            var query = new GetListGenericQuery<Client>(
                condition: c => c.IsActive,
                orderBy: q => q.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            );
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets inactive clients only
        /// </summary>
        [HttpGet("inactive")]
        [ProducesResponseType(typeof(IEnumerable<Client>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Client>>> GetInactive()
        {
            _logger.LogInformation("Getting all inactive clients");
            var query = new GetListGenericQuery<Client>(
                condition: c => !c.IsActive,
                orderBy: q => q.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            );
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
