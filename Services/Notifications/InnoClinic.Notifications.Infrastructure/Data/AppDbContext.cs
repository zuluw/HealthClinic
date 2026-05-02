using InnoClinic.Notifications.Domain.Models;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Notifications.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<InboxState>().ToTable("InboxState");
            modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage");
            modelBuilder.Entity<OutboxState>().ToTable("OutboxState");

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PatientFullName).IsRequired().HasMaxLength(300);
                entity.Property(e => e.PatientEmail).IsRequired().HasMaxLength(150);
                entity.Property(e => e.DoctorFullName).IsRequired().HasMaxLength(300);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(200);

                entity.Property(e => e.AppointmentDateTime).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });
        }
    }
}