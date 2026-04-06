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

        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserLoan> UserLoans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure LoanApplication Entity
            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ApplicantName).IsRequired().HasMaxLength(120);
                entity.Property(e => e.RequestedAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MonthlyIncome).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DecisionReason).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired();

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure User Entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.IsActive).IsRequired();

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure Role Entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(250);

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure UserRole Entity
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure UserLoan Entity
            modelBuilder.Entity<UserLoan>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserLoans)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LoanApplication)
                      .WithMany(l => l.UserLoans)
                      .HasForeignKey(e => e.LoanApplicationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.LoanApplicationId }).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}