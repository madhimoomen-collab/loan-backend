using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class ClientBook : BaseEntity
    {
        [Required(ErrorMessage = "Client ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Client ID must be greater than 0")]
        public int ClientId { get; set; }

        public Client Client { get; set; } = null!;

        [Required(ErrorMessage = "Book ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Book ID must be greater than 0")]
        public int BookId { get; set; }

        public Book Book { get; set; } = null!;

        [Required(ErrorMessage = "Borrowed date is required")]
        [DataType(DataType.DateTime)]
        public DateTime BorrowedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? ReturnedDate { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        public bool IsReturned { get; set; } = false;

        /// <summary>
        /// Calculated property: Is this book overdue?
        /// </summary>
        public bool IsOverdue => !IsReturned && DateTime.Now > DueDate;

        /// <summary>
        /// Calculated property: Days overdue (negative if not overdue)
        /// </summary>
        public int DaysOverdue => !IsReturned
            ? (DateTime.Now - DueDate).Days
            : 0;
    }
}