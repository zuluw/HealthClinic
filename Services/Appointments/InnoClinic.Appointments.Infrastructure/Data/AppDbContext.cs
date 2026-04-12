using InnoClinic.Appointments.Domain.Enums;
using InnoClinic.Appointments.Domain.Models;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Appointments.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Office> Offices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<InboxState>().ToTable("InboxState");
            modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage");
            modelBuilder.Entity<OutboxState>().ToTable("OutboxState");

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.HasQueryFilter(s => s.IsActive);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.HasOne(s => s.Specialization)
                    .WithMany(sp => sp.Services)
                    .HasForeignKey(s => s.SpecializationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(s => s.IsActive);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);

                entity.Property(e => e.OfficeId).IsRequired();

                entity.HasMany(d => d.Specializations)
                      .WithMany(s => s.Doctors)
                      .UsingEntity(j =>
                      {
                          j.ToTable("DoctorSpecializations");
                          j.Property<Guid>("DoctorId");
                          j.Property<Guid>("SpecializationId");
                      });

                entity.HasQueryFilter(d => d.Status == DoctorStatus.AtWork);
            });

            modelBuilder.Entity<Office>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(255);
                entity.HasQueryFilter(o => o.IsActive);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId);

                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId);

                entity.Property(e => e.PatientId).IsRequired();
                entity.Property(e => e.DateTime).IsRequired();
            });

            var officeId = "65f1a2b3c4d5e6f7a8b9c0d1";
            var specId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var doctorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var serviceId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            modelBuilder.Entity<Office>().HasData(
                new Office { Id = officeId, Address = "123 Main St, New York", IsActive = true }
            );

            modelBuilder.Entity<Specialization>().HasData(
                new Specialization { Id = specId, Name = "Cardiology", IsActive = true }
            );

            modelBuilder.Entity<Service>().HasData(
                new Service { Id = serviceId, Name = "General Consultation", Price = 100.00m, IsActive = true, SpecializationId = specId }
            );

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { Id = doctorId, FirstName = "Ivan", LastName = "Ivanov", MiddleName = "Ivanovich", OfficeId = officeId, Status = DoctorStatus.AtWork }
            );

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Specializations)
                .WithMany(s => s.Doctors)
                .UsingEntity<Dictionary<string, object>>(
                    "DoctorSpecializations",
                    j => j.HasOne<Specialization>().WithMany().HasForeignKey("SpecializationId"),
                    j => j.HasOne<Doctor>().WithMany().HasForeignKey("DoctorId"),
                    j =>
                    {
                        j.HasKey("DoctorId", "SpecializationId");
                        j.HasData(new { DoctorId = doctorId, SpecializationId = specId });
                    });
        }
    }
}