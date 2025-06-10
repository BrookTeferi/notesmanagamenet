using Microsoft.EntityFrameworkCore;
using NotesManagement.Models;

namespace NotesManagement.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define primary keys
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Note>().HasKey(n => n.Id);
            modelBuilder.Entity<RefreshToken>().HasKey(r => r.Id);

            // Configure relationships
            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId);

            // Seed user
            var userId = Guid.Parse("12345678-1234-5678-1234-567812345678");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = userId,
                Username = "alice",
                PasswordHash = "$2a$10$bw.8F9L7zLGn6RlZUlsrCe5T0ZRKm3XzZqWpZqxUuSdDQfGtIhV4y"
            });
        }
    }
}