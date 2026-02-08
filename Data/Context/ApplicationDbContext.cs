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

            modelBuilder.Entity<ClientBook>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Add UNIQUE constraint on ClientId + BookId
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