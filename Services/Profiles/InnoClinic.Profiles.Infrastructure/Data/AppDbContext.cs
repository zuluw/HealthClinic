using InnoClinic.Profiles.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Profiles.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Specialization> Specializations => Set<Specialization>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.AccountId).IsUnique();
                
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.AccountId).IsUnique();

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);

                entity.HasMany(d => d.Specializations)
                      .WithMany()
                      .UsingEntity(j =>
                      {
                          j.ToTable("DoctorSpecializations");
                          j.Property<Guid>("DoctorId");
                          j.Property<Guid>("SpecializationId");
                      });
            });

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            var surgeryId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var therapyId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            modelBuilder.Entity<Specialization>().HasData(
                new Specialization { Id = surgeryId, Name = "Surgery", IsActive = true },
                new Specialization { Id = therapyId, Name = "Therapy", IsActive = true }
            );
        }
    }
}