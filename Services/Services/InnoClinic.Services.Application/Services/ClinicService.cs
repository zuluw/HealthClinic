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
    public class ClinicService(IServiceRepository repository, ILogger<ClinicService> logger, ServiceMapper mapper, IValidator<SpecializationRequestDTO> validator, IValidator<UpdateSpecializationRequestDTO> updateSpecializationValidator, IValidator<UpdateServiceRequestDTO> updateServiceValidator) : IClinicService
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

            var validationResult = await updateSpecializationValidator.ValidateAsync(data, ct);
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

        public async Task<SpecializationResponseDTO> GetSpecializationByIdAsync(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching details for specialization {Id}", id);

            var specialization = await repository.GetSpecializationWithServicesByIdAsync(id, ct);

            if (specialization == null)
            {
                logger.LogWarning("Specialization {Id} not found", id);
                throw new NotFoundException("Specialization not found");
            }

            return mapper.ToSpecializationDto(specialization);
        }

        public async Task<List<SpecializationListDTO>> GetAllSpecializationsAsync(CancellationToken ct)
        {
            logger.LogInformation("Fetching all specializations for the table view");

            var specializations = await repository.GetAllSpecializationsAsync(ct);

            return specializations
                .Select(mapper.ToSpecializationListDto)
                .ToList();
        }

        public async Task UpdateServiceStatusAsync(Guid id, bool isActive, CancellationToken ct)
        {
            logger.LogInformation("Changing status for service {Id} to {Status}", id, isActive);

            var service = await repository.GetServiceByIdAsync(id, ct);

            if (service == null)
            {
                logger.LogWarning("Service {Id} not found", id);
                throw new NotFoundException("Service not found");
            }

            service.IsActive = isActive;

            await repository.SaveChangesAsync(ct);
            logger.LogInformation("Status for service {Id} successfully updated to {Status}", id, isActive);
        }

        public async Task UpdateServiceAsync(Guid id, UpdateServiceRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to update service {Id}", id);

            var validationResult = await updateServiceValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var service = await repository.GetServiceByIdAsync(id, ct);
            if (service == null)
            {
                throw new NotFoundException("Service not found");
            }

            mapper.UpdateService(data, service);

            await repository.SaveChangesAsync(ct);
            logger.LogInformation("Service {Id} successfully updated", id);
        }

        public async Task<ServiceResponseDTO> GetServiceByIdAsync(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Fetching details for service {Id}", id);

            var service = await repository.GetServiceByIdAsync(id, ct);

            if (service == null)
            {
                logger.LogWarning("Service {Id} not found", id);
                throw new NotFoundException("Service not found");
            }

            return mapper.ToServiceDto(service);
        }
    }
}