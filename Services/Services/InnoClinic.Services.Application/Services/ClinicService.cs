using FluentValidation;
using InnoClinic.Services.Application.DTOs;
using InnoClinic.Services.Application.Interfaces;
using InnoClinic.Services.Application.Mappings;
using InnoClinic.Services.Domain.Enums;
using InnoClinic.Services.Domain.Exceptions;
using InnoClinic.Services.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Services.Application.Services
{
    public class ClinicService(IServiceRepository repository, ILogger<ClinicService> logger, ServiceMapper mapper, IValidator<SpecializationRequestDTO> validator, IValidator<UpdateSpecializationRequestDTO> updateValidator) : IClinicService
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

        public async Task UpdateSpecializationAsync(Guid id, UpdateSpecializationRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to update specialization {Id}", id);

            var validationResult = await updateValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var specialization = await repository.GetSpecializationWithServicesByIdAsync(id, ct);
            if (specialization == null)
            {
                throw new NotFoundException("Specialization not found");
            }

            mapper.UpdateSpecialization(data, specialization);
            
            var requestServicesIds = data.Services.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
            var servicesToRemove = specialization.Services.Where(s => !requestServicesIds.Contains(s.Id)).ToList();

            foreach (var service in servicesToRemove)
            {
                repository.DeleteService(service);
            }

            foreach (var serviceDto in data.Services)
            {
                if (serviceDto.Id.HasValue)
                {
                    var existing = specialization.Services.FirstOrDefault(s => s.Id == serviceDto.Id);
                    if (existing != null)
                    {
                        existing.ServiceName = serviceDto.ServiceName;
                        existing.Price = serviceDto.Price;
                        existing.IsActive = serviceDto.IsActive;
                        existing.Category = serviceDto.Category;
                    }
                    else
                    {
                        throw new BadRequestException($"Service with ID {serviceDto.Id} not found.");
                    }
                }
                else
                {
                    var newService = new Service
                    {
                        ServiceName = serviceDto.ServiceName,
                        Price = serviceDto.Price,
                        IsActive = serviceDto.IsActive,
                        Category = serviceDto.Category,
                        SpecializationId = specialization.Id
                    };

                    repository.AddService(newService);
                }
            }

            await repository.SaveChangesAsync(ct);
            logger.LogInformation("Specialization {Id} updated successfully", id);
        }
    }
}