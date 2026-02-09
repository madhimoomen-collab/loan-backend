using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Book : BaseEntity
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
        [RegularExpression(@"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$",
            ErrorMessage = "Invalid ISBN format")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Published date is required")]
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Available copies must be a positive number")]
        public int AvailableCopies { get; set; } = 1;
    }
}