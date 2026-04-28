using InnoClinic.Documents.Domain.Models;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Documents.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<InboxState>().ToTable("InboxState");
            modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage");
            modelBuilder.Entity<OutboxState>().ToTable("OutboxState");

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.BlobName).IsRequired();
            });
        }
    }
}