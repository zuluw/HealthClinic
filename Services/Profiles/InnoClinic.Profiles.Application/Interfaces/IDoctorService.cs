using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Domain.Models;

namespace InnoClinic.Profiles.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<DoctorResponseDTO> CreateAsync(DoctorRequestDTO data, CancellationToken ct);
        Task<DoctorResponseDTO> GetByIdAsync(Guid id, CancellationToken ct);
        Task<DoctorResponseDTO> GetByAccountIdAsync(Guid accountId, CancellationToken ct); 
        Task<IEnumerable<DoctorResponseDTO>> GetBySpecializationAsync(Guid specializationId, CancellationToken ct);
        Task<IEnumerable<DoctorResponseDTO>> GetAllAsync(CancellationToken ct);
        Task UpdateAsync(Guid id, DoctorRequestDTO data, CancellationToken ct); 
        Task DeleteAsync(Guid id, CancellationToken ct); 
        Task ChangeStatusAsync(Guid id, DoctorStatus newStatus, CancellationToken ct);
    }
}