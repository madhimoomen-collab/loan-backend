using Domain.Commands;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientBooksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ClientBooksController> _logger;

        public ClientBooksController(IMediator mediator, ILogger<ClientBooksController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets all borrowed books (current and historical)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClientBook>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClientBook>>> GetAll()
        {
            _logger.LogInformation("Getting all client book records");
            var query = new GetListGenericQuery<ClientBook>();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific borrow record by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClientBook), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientBook>> GetById(int id)
        {
            _logger.LogInformation("Getting client book record with ID: {Id}", id);
            var query = new GetGenericQuery<ClientBook>(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Client book record with ID {id} not found.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new book borrowing record
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ClientBook), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClientBook>> BorrowBook([FromBody] ClientBook clientBook)
        {
            _logger.LogInformation("Client {ClientId} borrowing Book {BookId}",
                clientBook.ClientId, clientBook.BookId);

            // Set default values if not provided
            if (clientBook.BorrowedDate == default)
                clientBook.BorrowedDate = DateTime.Now;

            if (clientBook.DueDate == default)
                clientBook.DueDate = DateTime.Now.AddDays(14); // 2 week default

            clientBook.IsReturned = false;

            var command = new AddGenericCommand<ClientBook>(clientBook);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Returns a borrowed book
        /// </summary>
        [HttpPut("{id}/return")]
        [ProducesResponseType(typeof(ClientBook), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClientBook>> ReturnBook(int id)
        {
            _logger.LogInformation("Returning book with client book ID: {Id}", id);

            // Get the existing record
            var query = new GetGenericQuery<ClientBook>(id);
            var clientBook = await _mediator.Send(query);

            if (clientBook == null)
            {
                return NotFound($"Client book record with ID {id} not found.");
            }

            if (clientBook.IsReturned)
            {
                return BadRequest("This book has already been returned.");
            }

            // Update return information
            clientBook.IsReturned = true;
            clientBook.ReturnedDate = DateTime.Now;

            var command = new UpdateGenericCommand<ClientBook>(clientBook);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Updates a book borrowing record (e.g., extend due date)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ClientBook), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientBook>> Update(int id, [FromBody] ClientBook clientBook)
        {
            _logger.LogInformation("Updating client book record with ID: {Id}", id);

            clientBook.Id = id;
            var command = new UpdateGenericCommand<ClientBook>(clientBook);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a book borrowing record (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting client book record with ID: {Id}", id);

            var command = new DeleteGenericCommand<ClientBook>(id);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound($"Client book record with ID {id} not found.");
            }

            return NoContent();
        }
    }
}