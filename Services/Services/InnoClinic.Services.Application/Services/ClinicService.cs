using InnoClinic.Services.Application.DTOs;
using InnoClinic.Services.Application.Interfaces;
using InnoClinic.Services.Application.Mappings;
using InnoClinic.Services.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Services.Application.Services
{
    public class ClinicService(IServiceRepository repository, ILogger<ClinicService> logger, ServiceMapper mapper) : IClinicService
    {
        public async Task<List<SpecializationResponseDTO>> GetServicesAsync(ServiceCategory category, CancellationToken ct)
        {
            logger.LogInformation("Fetching services for category: {Category}", category);

            var specializations = await repository.GetSpecializationsWithServicesAsync(category, ct);

            return specializations
                .Select(mapper.ToSpecializationDto)
                .ToList();
        }
    }
}