using AutoMapper;
using Domain.Commands;
using Domain.DTOs;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing book reservations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IMediator mediator,
            IMapper mapper,
            ILogger<ReservationsController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all reservations with optional filtering
        /// </summary>
        /// <param name="status">Filter by status (Active, PickedUp, Cancelled, Expired)</param>
        /// <param name="clientId">Filter by client ID</param>
        /// <param name="bookId">Filter by book ID</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReservationListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservationListDto>>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] int? clientId = null,
            [FromQuery] int? bookId = null)
        {
            _logger.LogInformation("Getting reservations - Status: {Status}, ClientId: {ClientId}, BookId: {BookId}",
                status, clientId, bookId);

            var query = new GetListGenericQuery<Reservation>(
                condition: r =>
                    (status == null || r.Status.ToString() == status) &&
                    (clientId == null || r.ClientId == clientId) &&
                    (bookId == null || r.BookId == bookId),
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book),
                orderBy: q => q.OrderByDescending(r => r.ReservationDate)
            );

            var reservations = await _mediator.Send(query);
            var dto = _mapper.Map<IEnumerable<ReservationListDto>>(reservations);

            return Ok(dto);
        }

        /// <summary>
        /// Get a specific reservation by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservationDto>> GetById(int id)
        {
            _logger.LogInformation("Getting reservation with ID: {Id}", id);

            var query = new GetGenericQuery<Reservation>(
                condition: r => r.Id == id,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book)
            );

            var reservation = await _mediator.Send(query);

            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            var dto = _mapper.Map<ReservationDto>(reservation);
            return Ok(dto);
        }

        /// <summary>
        /// Create a new book reservation
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationDto createDto)
        {
            _logger.LogInformation("Creating reservation - Client: {ClientId}, Book: {BookId}",
                createDto.ClientId, createDto.BookId);

            // Validate client exists
            var clientQuery = new GetGenericQuery<Client>(createDto.ClientId);
            var client = await _mediator.Send(clientQuery);
            if (client == null)
            {
                return BadRequest($"Client with ID {createDto.ClientId} not found.");
            }

            if (!client.IsActive)
            {
                return BadRequest("Client account is not active.");
            }

            // Validate book exists and is available
            var bookQuery = new GetGenericQuery<Book>(createDto.BookId);
            var book = await _mediator.Send(bookQuery);
            if (book == null)
            {
                return BadRequest($"Book with ID {createDto.BookId} not found.");
            }

            if (book.AvailableCopies <= 0)
            {
                return BadRequest("Book is not available for reservation.");
            }

            // Check if client already has an active reservation for this book
            var existingReservationQuery = new GetListGenericQuery<Reservation>(
                condition: r => r.ClientId == createDto.ClientId
                    && r.BookId == createDto.BookId
                    && r.Status == ReservationStatus.Active
            );
            var existingReservations = await _mediator.Send(existingReservationQuery);

            if (existingReservations.Any())
            {
                return BadRequest("You already have an active reservation for this book.");
            }

            // Create the reservation
            var reservation = _mapper.Map<Reservation>(createDto);
            var command = new AddGenericCommand<Reservation>(reservation);
            var result = await _mediator.Send(command);

            // Reload with navigation properties
            var reloadQuery = new GetGenericQuery<Reservation>(
                condition: r => r.Id == result.Id,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book)
            );
            var createdReservation = await _mediator.Send(reloadQuery);

            var dto = _mapper.Map<ReservationDto>(createdReservation);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, dto);
        }

        /// <summary>
        /// Update a reservation (extend expiry date or update notes)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReservationDto>> Update(
            int id,
            [FromBody] UpdateReservationDto updateDto)
        {
            _logger.LogInformation("Updating reservation with ID: {Id}", id);

            var query = new GetGenericQuery<Reservation>(
                condition: r => r.Id == id,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book)
            );
            var reservation = await _mediator.Send(query);

            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            if (reservation.Status != ReservationStatus.Active)
            {
                return BadRequest("Can only update active reservations.");
            }

            // Extend expiry date if requested
            if (updateDto.ExtendByDays.HasValue)
            {
                reservation.ExpiryDate = reservation.ExpiryDate.AddDays(updateDto.ExtendByDays.Value);
            }

            // Update notes if provided
            if (updateDto.Notes != null)
            {
                reservation.Notes = updateDto.Notes;
            }

            var command = new UpdateGenericCommand<Reservation>(reservation);
            var result = await _mediator.Send(command);

            var dto = _mapper.Map<ReservationDto>(result);
            return Ok(dto);
        }

        /// <summary>
        /// Cancel a reservation
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReservationDto>> Cancel(int id)
        {
            _logger.LogInformation("Cancelling reservation with ID: {Id}", id);

            var query = new GetGenericQuery<Reservation>(
                condition: r => r.Id == id,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book)
            );
            var reservation = await _mediator.Send(query);

            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            if (reservation.Status != ReservationStatus.Active)
            {
                return BadRequest("Can only cancel active reservations.");
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.CancelledDate = DateTime.Now;

            var command = new UpdateGenericCommand<Reservation>(reservation);
            var result = await _mediator.Send(command);

            var dto = _mapper.Map<ReservationDto>(result);
            return Ok(dto);
        }

        /// <summary>
        /// Mark reservation as picked up (convert to borrowing)
        /// </summary>
        [HttpPost("{id}/pickup")]
        [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReservationDto>> Pickup(int id)
        {
            _logger.LogInformation("Processing pickup for reservation ID: {Id}", id);

            var query = new GetGenericQuery<Reservation>(
                condition: r => r.Id == id,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book)
            );
            var reservation = await _mediator.Send(query);

            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            if (reservation.Status != ReservationStatus.Active)
            {
                return BadRequest("Reservation is not active.");
            }

            if (reservation.IsExpired)
            {
                return BadRequest("Reservation has expired.");
            }

            reservation.Status = ReservationStatus.PickedUp;
            reservation.PickupDate = DateTime.Now;

            var command = new UpdateGenericCommand<Reservation>(reservation);
            var result = await _mediator.Send(command);

            var dto = _mapper.Map<ReservationDto>(result);
            return Ok(dto);
        }

        /// <summary>
        /// Get all reservations for a specific client
        /// </summary>
        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(IEnumerable<ReservationListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservationListDto>>> GetByClient(int clientId)
        {
            _logger.LogInformation("Getting reservations for client ID: {ClientId}", clientId);

            var query = new GetListGenericQuery<Reservation>(
                condition: r => r.ClientId == clientId,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book),
                orderBy: q => q.OrderByDescending(r => r.ReservationDate)
            );

            var reservations = await _mediator.Send(query);
            var dto = _mapper.Map<IEnumerable<ReservationListDto>>(reservations);

            return Ok(dto);
        }

        /// <summary>
        /// Get all reservations for a specific book
        /// </summary>
        [HttpGet("book/{bookId}")]
        [ProducesResponseType(typeof(IEnumerable<ReservationListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservationListDto>>> GetByBook(int bookId)
        {
            _logger.LogInformation("Getting reservations for book ID: {BookId}", bookId);

            var query = new GetListGenericQuery<Reservation>(
                condition: r => r.BookId == bookId,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book),
                orderBy: q => q.OrderByDescending(r => r.ReservationDate)
            );

            var reservations = await _mediator.Send(query);
            var dto = _mapper.Map<IEnumerable<ReservationListDto>>(reservations);

            return Ok(dto);
        }

        /// <summary>
        /// Get all active reservations
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<ReservationListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservationListDto>>> GetActive()
        {
            _logger.LogInformation("Getting all active reservations");

            var query = new GetListGenericQuery<Reservation>(
                condition: r => r.Status == ReservationStatus.Active,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book),
                orderBy: q => q.OrderBy(r => r.ExpiryDate)
            );

            var reservations = await _mediator.Send(query);
            var dto = _mapper.Map<IEnumerable<ReservationListDto>>(reservations);

            return Ok(dto);
        }

        /// <summary>
        /// Get all expired reservations
        /// </summary>
        [HttpGet("expired")]
        [ProducesResponseType(typeof(IEnumerable<ReservationListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservationListDto>>> GetExpired()
        {
            _logger.LogInformation("Getting all expired reservations");

            var query = new GetListGenericQuery<Reservation>(
                condition: r => r.Status == ReservationStatus.Active
                    && r.ExpiryDate < DateTime.Now,
                includes: q => q
                    .Include(r => r.Client)
                    .Include(r => r.Book),
                orderBy: q => q.OrderBy(r => r.ExpiryDate)
            );

            var reservations = await _mediator.Send(query);
            var dto = _mapper.Map<IEnumerable<ReservationListDto>>(reservations);

            return Ok(dto);
        }

        /// <summary>
        /// Delete a reservation (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting reservation with ID: {Id}", id);

            var command = new DeleteGenericCommand<Reservation>(id);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            return NoContent();
        }
    }
}