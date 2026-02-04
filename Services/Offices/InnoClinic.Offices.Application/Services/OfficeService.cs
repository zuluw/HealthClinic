using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Application.Interfaces;
using InnoClinic.Offices.Domain.Exceptions;
using InnoClinic.Offices.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Offices.Application.Services
{
    public class OfficeService(IOfficeRepository repository, ILogger<OfficeService> logger) : IOfficeService
    {
        public async Task<IEnumerable<Office>> GetAllAsync()
        {
            return await repository.GetAllAsync();
        }

        public async Task<Office?> GetByIdAsync(string id)
        {
            return await repository.GetByIdAsync(id);
        }

        public async Task<Office> CreateAsync(OfficeDto data)
        {
            logger.LogInformation("Creating a new office at address: {Address}", data.Address);

            var office = new Office
            {
                Address = data.Address,
                RegistryPhoneNumber = data.RegistryPhoneNumber,
                IsActive = data.IsActive
            };

            await repository.CreateAsync(office);

            logger.LogInformation("Office successfully created with ID: {Id}", office.Id);
            return office;
        }

        public async Task UpdateAsync(string id, OfficeDto data)
        {
            logger.LogInformation("Updating office {Id}. New address: {Address}", id, data.Address);

            var existingOffice = await repository.GetByIdAsync(id);
            if (existingOffice == null)
            {
                logger.LogError("Update failed. Office {Id} not found", id);
                throw new NotFoundException("Office not found");
            }

            existingOffice.Address = data.Address;
            existingOffice.RegistryPhoneNumber = data.RegistryPhoneNumber;
            existingOffice.IsActive = data.IsActive;

            await repository.UpdateAsync(id, existingOffice);
            logger.LogInformation("Office {Id} information updated", id);
        }

        public async Task DeleteAsync(string id)
        {
            logger.LogWarning("Attempting to delete office with ID: {Id}", id);

            var existingOffice = await repository.GetByIdAsync(id);
            if (existingOffice == null)
            {
                throw new NotFoundException("Office not found");
            }

            await repository.DeleteAsync(id);
            logger.LogInformation("Office {Id} deleted", id);
        }
    }
}