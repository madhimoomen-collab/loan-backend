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
        public DbSet<Reservation> Reservations { get; set; } // NEW

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

                entity.HasQueryFilter(c => !c.IsDeleted);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure ClientBook Entity (Junction Table)
            modelBuilder.Entity<ClientBook>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ClientId, e.BookId }).IsUnique();

                entity.HasOne(cb => cb.Client)
                      .WithMany()
                      .HasForeignKey(cb => cb.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cb => cb.Book)
                      .WithMany()
                      .HasForeignKey(cb => cb.BookId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(cb => !cb.IsDeleted);
            });

            // Configure Reservation Entity (NEW)
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ReservationDate).IsRequired();
                entity.Property(e => e.ExpiryDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Relationships
                entity.HasOne(r => r.Client)
                      .WithMany()
                      .HasForeignKey(r => r.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Book)
                      .WithMany()
                      .HasForeignKey(r => r.BookId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.BookId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ExpiryDate);
                entity.HasIndex(e => new { e.ClientId, e.BookId, e.Status });

                // Soft delete filter
                entity.HasQueryFilter(r => !r.IsDeleted);
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