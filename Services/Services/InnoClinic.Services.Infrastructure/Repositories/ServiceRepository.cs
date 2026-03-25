using InnoClinic.Services.Application.Interfaces;
using InnoClinic.Services.Domain.Enums;
using InnoClinic.Services.Domain.Models;
using InnoClinic.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Services.Infrastructure.Repositories
{
    public class ServiceRepository(AppDbContext context) : IServiceRepository
    {
        public async Task<List<Service>> GetByCategoryAsync(ServiceCategory category, CancellationToken ct)
        {
            return await context.Services
                .AsNoTracking()
                .Where(s => s.Category == category)
                .Include(s => s.Specialization)
                .ToListAsync(ct);
        }

        public async Task<List<Specialization>> GetSpecializationsWithServicesAsync(ServiceCategory category, CancellationToken ct)
        {
            return await context.Specializations
                .AsNoTracking()
                .Include(sp => sp.Services.Where(s => s.Category == category))
                .Where(sp => sp.Services.Any(s => s.Category == category))
                .ToListAsync(ct);
        }

        public async Task<bool> SpecializationExistsAsync(string name, CancellationToken ct)
        {
            return await context.Specializations.AnyAsync(s => s.Name == name, ct);
        }

        public void AddSpecialization(Specialization specialization)
        {
            context.Specializations.Add(specialization);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}