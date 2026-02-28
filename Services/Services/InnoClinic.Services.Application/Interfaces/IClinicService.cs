using InnoClinic.Services.Application.DTOs;
using InnoClinic.Services.Domain.Enums;

namespace InnoClinic.Services.Application.Interfaces;

public interface IClinicService
{
    Task<List<SpecializationResponseDTO>> GetServicesAsync(ServiceCategory category, CancellationToken ct);
}