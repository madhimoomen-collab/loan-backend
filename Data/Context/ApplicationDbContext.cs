using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.HasQueryFilter(b => !b.IsDeleted);
                entity.HasIndex(e => e.ISBN).IsUnique();
            });

            // FIX APPLIED BELOW: Replaced 'DateTime.Now' with static 'new DateTime(...)'
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Clean Code",
                    ISBN = "978-0132350884",
                    Author = "Robert C. Martin",
                    PublishedDate = new DateTime(2008, 8, 1),
                    Category = "Programming",
                    AvailableCopies = 5,
                    CreatedDate = new DateTime(2024, 1, 1) // Fixed: Static Date
                },
                new Book
                {
                    Id = 2,
                    Title = "Design Patterns",
                    ISBN = "978-0201633612",
                    Author = "Gang of Four",
                    PublishedDate = new DateTime(1994, 10, 31),
                    Category = "Software Engineering",
                    AvailableCopies = 3,
                    CreatedDate = new DateTime(2024, 1, 1) // Fixed: Static Date
                }
            );
        }
    }
}