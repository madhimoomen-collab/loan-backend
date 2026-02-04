using MediatR;
using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using Domain.Commands;
using Domain.Queries;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IMediator mediator, ILogger<BooksController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Book>>> GetAll()
        {
            _logger.LogInformation("Getting all books");
            var query = new GetListGenericQuery<Book>();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Book>> GetById(int id)
        {
            _logger.LogInformation("Getting book with ID: {Id}", id);
            var query = new GetGenericQuery<Book>(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Book>> Create([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new book: {Title}", book.Title);
            var command = new AddGenericCommand<Book>(book);
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Book>> Update(int id, [FromBody] Book book)
        {
            if (id != book.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating book with ID: {Id}", id);
            var command = new UpdateGenericCommand<Book>(book);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting book with ID: {Id}", id);
            var command = new DeleteGenericCommand<Book>(id);
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Book with ID {Id} not found for deletion", id);
                return NotFound(new { message = $"Book with ID {id} not found" });
            }

            return NoContent();
        }
    }
}