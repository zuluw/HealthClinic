using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Domain.Models;

namespace InnoClinic.Offices.Application.Interfaces;

public interface IOfficeService
{
    Task<IEnumerable<Office>> GetAllAsync();
    Task<Office?> GetByIdAsync(string id);
    Task<Office> CreateAsync(OfficeDto officeDto);
    Task UpdateAsync(string id, OfficeDto officeDto);
    Task DeleteAsync(string id);
}