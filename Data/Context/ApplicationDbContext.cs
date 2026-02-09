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
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientBook> ClientBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Book Entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);

                // Global query filter for soft delete
                entity.HasQueryFilter(b => !b.IsDeleted);

                entity.HasIndex(e => e.ISBN).IsUnique();
            });

            // Configure Client Entity
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(300);

                // FIXED: Added query filter for soft delete (was missing!)
                entity.HasQueryFilter(c => !c.IsDeleted);

                // Add unique constraint on Email
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure ClientBook Entity (Junction Table)
            modelBuilder.Entity<ClientBook>(entity =>
            {
                entity.HasKey(e => e.Id);

                // UNIQUE constraint on ClientId + BookId to prevent duplicate borrowing
                entity.HasIndex(e => new { e.ClientId, e.BookId }).IsUnique();

                // Configure relationships
                entity.HasOne(cb => cb.Client)
                      .WithMany()
                      .HasForeignKey(cb => cb.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cb => cb.Book)
                      .WithMany()
                      .HasForeignKey(cb => cb.BookId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Global query filter for soft delete
                entity.HasQueryFilter(cb => !cb.IsDeleted);
            });

            // Seed Data
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
                    CreatedDate = new DateTime(2024, 1, 1)
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
                    CreatedDate = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}