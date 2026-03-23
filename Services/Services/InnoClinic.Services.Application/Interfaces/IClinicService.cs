using InnoClinic.Services.Application.DTOs;
using InnoClinic.Services.Domain.Enums;

namespace InnoClinic.Services.Application.Interfaces;

public interface IClinicService
{
    Task<List<SpecializationResponseDTO>> GetServicesAsync(ServiceCategory category, CancellationToken ct);
    Task<Guid> CreateSpecializationAsync(SpecializationRequestDTO data, CancellationToken ct);
    Task UpdateSpecializationStatusAsync(Guid id, bool isActive, CancellationToken ct);
    Task UpdateSpecializationAsync(Guid id, UpdateSpecializationRequestDTO data, CancellationToken ct);
    Task<SpecializationResponseDTO> GetSpecializationByIdAsync(Guid id, CancellationToken ct);
    Task<List<SpecializationListDTO>> GetAllSpecializationsAsync(CancellationToken ct);
}