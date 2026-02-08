using System;

namespace Domain.Models
{
    public class ClientBook : BaseEntity
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public DateTime BorrowedDate { get; set; } = DateTime.Now;
        public DateTime? ReturnedDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsReturned { get; set; } = false;
    }
}