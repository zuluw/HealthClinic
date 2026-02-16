using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Domain.Models;
using InnoClinic.Profiles.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Profiles.Infrastructure.Repositories;

public class PatientRepository(ProfilesDbContext context) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Patient?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await context.Patients.FirstOrDefaultAsync(p => p.AccountId == accountId, cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Patients.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken)
    {
        await context.Patients.AddAsync(patient, cancellationToken);
    }

    public void Update(Patient patient)
    {
        context.Patients.Update(patient);
    }

    public void Delete(Patient patient)
    {
        context.Patients.Remove(patient);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}