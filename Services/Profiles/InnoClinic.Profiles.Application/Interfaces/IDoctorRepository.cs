using InnoClinic.Profiles.Domain.Models;

namespace InnoClinic.Profiles.Application.Interfaces
{

    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Doctor>> GetBySpecializationAsync(Guid specializationId, CancellationToken cancellationToken);
        Task AddAsync(Doctor doctor, CancellationToken cancellationToken);
        void Update(Doctor doctor);
        void Delete(Doctor doctor);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}