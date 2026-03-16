using FluentValidation;
using InnoClinic.Services.Application.DTOs;
using InnoClinic.Services.Application.Interfaces;
using InnoClinic.Services.Application.Mappings;
using InnoClinic.Services.Domain.Enums;
using InnoClinic.Services.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Services.Application.Services
{
    public class ClinicService(IServiceRepository repository, ILogger<ClinicService> logger, ServiceMapper mapper, IValidator<SpecializationRequestDTO> validator) : IClinicService
    {
        public async Task<List<SpecializationResponseDTO>> GetServicesAsync(ServiceCategory category, CancellationToken ct)
        {
            logger.LogInformation("Fetching services for category: {Category}", category);

            var specializations = await repository.GetSpecializationsWithServicesAsync(category, ct);

            return specializations
                .Select(mapper.ToSpecializationDto)
                .ToList();
        }

        public async Task<Guid> CreateSpecializationAsync(SpecializationRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to create a new specialization: {SpecName}", data.Name);

            var validationResult = await validator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for specialization: {SpecName}. Errors: {Errors}", data.Name, string.Join(", ", validationResult.Errors));
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var exists = await repository.SpecializationExistsAsync(data.Name, ct);
            
            if (exists)
            {
                logger.LogWarning("Specialization creation failed. Name already exists: {SpecName}", data.Name);
                throw new BadRequestException("Specialization already exists");
            }

            var specialization = mapper.ToEntity(data);

            repository.AddSpecialization(specialization);
            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Specialization {SpecName} successfully created with ID: {Id}", specialization.Name, specialization.Id);

            return specialization.Id;
        }

        public async Task UpdateSpecializationStatusAsync(Guid id, bool isActive, CancellationToken ct)
        {
            logger.LogInformation("Changing status for specialization {Id} to {Status}", id, isActive);

            var specialization = await repository.GetSpecializationWithServicesByIdAsync(id, ct);

            if (specialization == null)
            {
                logger.LogWarning("Specialization {Id} not found", id);
                throw new NotFoundException("Specialization not found");
            }

            specialization.IsActive = isActive;

            if (!isActive)
            {
                foreach (var service in specialization.Services)
                {
                    service.IsActive = false;
                }
                logger.LogInformation("Cascade deactivated {Count} services for specialization {Id}", specialization.Services.Count, id);
            }

            await repository.SaveChangesAsync(ct);
        }
    }
}