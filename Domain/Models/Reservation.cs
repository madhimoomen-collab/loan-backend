using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    /// <summary>
    /// Represents a book reservation by a client
    /// A reservation holds a book for a client before they borrow it
    /// </summary>
    public class Reservation : BaseEntity
    {
        [Required(ErrorMessage = "Client ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Client ID must be greater than 0")]
        public int ClientId { get; set; }

        /// <summary>
        /// Navigation property to Client
        /// </summary>
        public Client? Client { get; set; }

        [Required(ErrorMessage = "Book ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Book ID must be greater than 0")]
        public int BookId { get; set; }

        /// <summary>
        /// Navigation property to Book
        /// </summary>
        public Book? Book { get; set; }

        [Required(ErrorMessage = "Reservation date is required")]
        [DataType(DataType.DateTime)]
        public DateTime ReservationDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Expiry date is required")]
        [DataType(DataType.DateTime)]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Reservation status
        /// </summary>
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        /// <summary>
        /// Date when the reservation was picked up (converted to borrowing)
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? PickupDate { get; set; }

        /// <summary>
        /// Date when the reservation was cancelled
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? CancelledDate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        /// <summary>
        /// NEW: Link to the ClientBook record created when this reservation was picked up
        /// This tracks which borrowing came from this reservation
        /// </summary>
        public int? ClientBookId { get; set; }

        /// <summary>
        /// NEW: Navigation property to the borrowing record created from this reservation
        /// </summary>
        public ClientBook? ClientBook { get; set; }

        /// <summary>
        /// Calculated property: Is this reservation expired?
        /// </summary>
        public bool IsExpired => Status == ReservationStatus.Active && DateTime.Now > ExpiryDate;

        /// <summary>
        /// Calculated property: Days until expiry (negative if expired)
        /// </summary>
        public int DaysUntilExpiry => (ExpiryDate - DateTime.Now).Days;
    }

    /// <summary>
    /// Reservation status enumeration
    /// </summary>
    public enum ReservationStatus
    {
        /// <summary>
        /// Reservation is active and waiting for pickup
        /// </summary>
        Active = 0,

        /// <summary>
        /// Reservation was picked up and converted to borrowing
        /// </summary>
        PickedUp = 1,

        /// <summary>
        /// Reservation was cancelled by client or system
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// Reservation expired (not picked up in time)
        /// </summary>
        Expired = 3
    }
}