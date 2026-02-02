using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Application.Interfaces;
using InnoClinic.Offices.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Offices.Application.Services;

public class OfficeService : IOfficeService
{
    private readonly IOfficeRepository _repository;
    private readonly ILogger<OfficeService> _logger;

    public OfficeService(IOfficeRepository repository, ILogger<OfficeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Office>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Office?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Office> CreateAsync(OfficeDto officeDto)
    {
        _logger.LogInformation("Creating a new office at address: {Address}", officeDto.Address);

        var office = new Office
        {
            Address = officeDto.Address,
            RegistryPhoneNumber = officeDto.RegistryPhoneNumber,
            IsActive = officeDto.IsActive
        };

        await _repository.CreateAsync(office);

        _logger.LogInformation("Office successfully created with ID: {Id}", office.Id);
        return office;
    }

    public async Task UpdateAsync(string id, OfficeDto officeDto)
    {
        _logger.LogInformation("Updating office {Id}. New address: {Address}", id, officeDto.Address);

        var existingOffice = await _repository.GetByIdAsync(id);
        if (existingOffice == null)
        {
            _logger.LogError("Update failed. Office {Id} not found", id);
            throw new Exception("Office not found!");
        }

        existingOffice.Address = officeDto.Address;
        existingOffice.RegistryPhoneNumber = officeDto.RegistryPhoneNumber;
        existingOffice.IsActive = officeDto.IsActive;

        await _repository.UpdateAsync(id, existingOffice);
        _logger.LogInformation("Office {Id} information updated", id);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogWarning("Attempting to delete office with ID: {Id}", id);
        
        var existingOffice = await _repository.GetByIdAsync(id);
        if (existingOffice == null) 
            throw new Exception("Office not found!");

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Office {Id} deleted", id);
    }
}