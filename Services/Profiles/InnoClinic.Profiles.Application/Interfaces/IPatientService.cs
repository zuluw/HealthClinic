using InnoClinic.Profiles.Application.DTOs;

namespace InnoClinic.Profiles.Application.Interfaces
{
    public interface IPatientService
    {
        Task<PatientResponseDTO> CreateAsync(PatientRequestDTO data, CancellationToken ct);
        Task<PatientResponseDTO> GetByIdAsync(Guid id, CancellationToken ct);
        Task<PatientResponseDTO> GetByAccountIdAsync(Guid accountId, CancellationToken ct);
        Task<IEnumerable<PatientResponseDTO>> GetAllAsync(CancellationToken ct);
        Task UpdateAsync(Guid id, PatientRequestDTO data, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}