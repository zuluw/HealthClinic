using InnoClinic.Profiles.Domain.Models;

namespace InnoClinic.Profiles.Application.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Doctor?> GetByAccountIdAsync(Guid accountId, CancellationToken ct);
        Task<IEnumerable<Doctor>> GetBySpecializationAsync(Guid specializationId, CancellationToken ct);
        Task<IEnumerable<Specialization>> GetSpecializationsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
        Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken ct);
        void Add(Doctor doctor);
        void Update(Doctor doctor);
        void Delete(Doctor doctor);
        Task SaveChangesAsync(CancellationToken ct);
    }
}