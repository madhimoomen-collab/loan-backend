using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs
{
    /// <summary>
    /// DTO for creating a new book reservation
    /// </summary>
    public class CreateReservationDto
    {
        [Required(ErrorMessage = "Client ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Client ID must be greater than 0")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Book ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Book ID must be greater than 0")]
        public int BookId { get; set; }

        /// <summary>
        /// Number of days to hold the reservation (default 3 days)
        /// </summary>
        [Range(1, 30, ErrorMessage = "Reservation duration must be between 1 and 30 days")]
        public int ReservationDurationDays { get; set; } = 3;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for reservation response (output)
    /// </summary>
    public class ReservationDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;

        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime? PickupDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? Notes { get; set; }

        // Calculated fields
        public bool IsExpired { get; set; }
        public int DaysUntilExpiry { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// DTO for updating a reservation
    /// </summary>
    public class UpdateReservationDto
    {
        [Range(1, 30, ErrorMessage = "Reservation duration must be between 1 and 30 days")]
        public int? ExtendByDays { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Simplified DTO for listing reservations
    /// </summary>
    public class ReservationListDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
    }
}