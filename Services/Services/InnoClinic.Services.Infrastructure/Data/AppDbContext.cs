using InnoClinic.Services.Domain.Enums;
using InnoClinic.Services.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace InnoClinic.Services.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Service> Services { get; set; }
        public DbSet<Specialization> Specializations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);

                entity.HasQueryFilter(s => s.IsActive);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(200);

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.HasOne(s => s.Specialization)
                    .WithMany(sp => sp.Services)
                    .HasForeignKey(s => s.SpecializationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(s => s.IsActive);
            });

            var specId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            modelBuilder.Entity<Specialization>().HasData(
                new Specialization { Id = specId, Name = "Cardiology", IsActive = true }
            );

            modelBuilder.Entity<Service>().HasData(
                new Service
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ServiceName = "Consultation with cardiologist",
                    Price = 150.00m,
                    IsActive = true,
                    Category = ServiceCategory.Consultations,
                    SpecializationId = specId
                },
                new Service
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    ServiceName = "Blood Test",
                    Price = 20.00m,
                    IsActive = true,
                    Category = ServiceCategory.Analyses,
                    SpecializationId = specId
                }
            );
        }
    }
}