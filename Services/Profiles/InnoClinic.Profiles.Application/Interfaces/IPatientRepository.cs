using InnoClinic.Profiles.Domain.Models;

namespace InnoClinic.Profiles.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Patient?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);
        Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken);
        void Add(Patient patient);
        void Update(Patient patient);
        void Delete(Patient patient);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}