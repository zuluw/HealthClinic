using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Domain.Models;
using InnoClinic.Profiles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Profiles.Infrastructure.Repositories;

public class DoctorRepository(AppDbContext context) : IDoctorRepository
{
    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Doctors
            .Include(d => d.Specializations)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Doctor?> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        return await context.Doctors
            .Include(d => d.Specializations)
            .FirstOrDefaultAsync(d => d.AccountId == accountId, ct);
    }

    public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(Guid specializationId, CancellationToken cancellationToken)
    {
        return await context.Doctors
            .Include(d => d.Specializations)
            .Where(d => d.Specializations.Any(s => s.Id == specializationId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Specialization>> GetSpecializationsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        return await context.Specializations
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken ct)
    {
        return await context.Doctors
            .Include(d => d.Specializations)
            .ToListAsync(ct);
    }

    public void Add(Doctor doctor)
    {
        context.Doctors.Add(doctor);
    }

    public void Update(Doctor doctor)
    {
        context.Doctors.Update(doctor);
    }

    public void Delete(Doctor doctor)
    {
        context.Doctors.Remove(doctor);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}