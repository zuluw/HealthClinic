using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Domain.Models;
using InnoClinic.Profiles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Profiles.Infrastructure.Repositories;

public class DoctorRepository(ProfilesDbContext context) : IDoctorRepository
{
    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Doctors
            .Include(d => d.Specializations)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(Guid specializationId, CancellationToken cancellationToken)
    {
        return await context.Doctors
            .Include(d => d.Specializations)
            .Where(d => d.Specializations.Any(s => s.Id == specializationId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken)
    {
        await context.Doctors.AddAsync(doctor, cancellationToken);
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