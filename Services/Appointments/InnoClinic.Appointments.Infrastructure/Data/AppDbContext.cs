using InnoClinic.Appointments.Domain.Enums;
using InnoClinic.Appointments.Domain.Models;
using MassTransit;
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
        }
    }
}