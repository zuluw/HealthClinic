using Microsoft.EntityFrameworkCore;
using InnoClinic.Auth.Domain.Models;

namespace InnoClinic.Auth.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            var passHash = "$2a$11$XcnlxYM.BJ8YPPS25N5CH.il8J6hxW/umZvgHVQ3dG8qR1bkc7iNq";
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserName = "admin",
                    Email = "admin@innoclinic.com",
                    PasswordHash = passHash,
                    FirstName = "System",
                    LastName = "Admin",
                    Status = UserStatus.Confirmed,
                    CreatedAt = seedDate
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    UserName = "Zuluw",
                    Email = "sashka@mail.ru",
                    PasswordHash = passHash,
                    FirstName = "Aleksandr",
                    LastName = "Mikhalkov",
                    Status = UserStatus.Unconfirmed,
                    CreatedAt = seedDate
                }
            );
        }
    }
}
