using FluentValidation;
using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Application.Interfaces;
using InnoClinic.Offices.Application.Mappings;
using InnoClinic.Offices.Domain.Exceptions;
using InnoClinic.Offices.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Offices.Application.Services
{
    public class OfficeService(IOfficeRepository repository, IValidator<OfficeRequestDTO> officeValidator, ILogger<OfficeService> logger, OfficeMapper mapper) : IOfficeService
    {
        public async Task<PagedResult<OfficeResponseDTO>> GetAllAsync(PaginationParameters parameters, CancellationToken ct)
        {
            var (items, totalCount) = await repository.GetAllAsync(parameters, ct);

            var result = mapper.ToResponseList(items);

            return new PagedResult<OfficeResponseDTO>(result, parameters.PageNumber, PaginationParameters.PageSize, totalCount);
        }

        public async Task<OfficeResponseDTO?> GetByIdAsync(string id, CancellationToken ct)
        {
            var office = await repository.GetByIdAsync(id, ct);
            return office is null ? null : mapper.ToResponse(office);
        }

        public async Task<OfficeResponseDTO> CreateAsync(OfficeRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Creating a new office at address: {Address}", data.Address);

            var validationResult = await officeValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for office creation at address: {Address}", data.Address);
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var office = mapper.ToEntity(data);

            await repository.CreateAsync(office, ct);

            logger.LogInformation("Office successfully created with ID: {Id}", office.Id);
            return mapper.ToResponse(office);
        }

        public async Task UpdateAsync(string id, OfficeRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Updating office {Id}. New address: {Address}", id, data.Address);

            var validationResult = await officeValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for office update at ID: {Id}", id);
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var existingOffice = await repository.GetByIdAsync(id, ct);
            if (existingOffice is null)
            {
                logger.LogError("Update failed. Office {Id} not found", id);
                throw new NotFoundException("Office not found");
            }

            mapper.UpdateEntity(data, existingOffice);

            await repository.UpdateAsync(id, existingOffice, ct);
            logger.LogInformation("Office {Id} information updated", id);
        }

        public async Task DeleteAsync(string id, CancellationToken ct)
        {
            logger.LogWarning("Attempting to delete office with ID: {Id}", id);

            var existingOffice = await repository.GetByIdAsync(id, ct);
            if (existingOffice is null)
            {
                throw new NotFoundException("Office not found");
            }

            await repository.DeleteAsync(id, ct);
            logger.LogInformation("Office {Id} deleted", id);
        }
    }
}