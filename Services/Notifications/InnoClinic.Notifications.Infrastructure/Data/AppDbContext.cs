using Microsoft.EntityFrameworkCore;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

namespace InnoClinic.Notifications.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<InboxState>().ToTable("InboxState");
            modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage");
            modelBuilder.Entity<OutboxState>().ToTable("OutboxState");
        }
    }
}