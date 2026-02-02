using InnoClinic.Offices.Domain.Models;

namespace InnoClinic.Offices.Application.Interfaces;

public interface IOfficeRepository
{
    Task<IEnumerable<Office>> GetAllAsync();
    Task<Office?> GetByIdAsync(string id);
    Task CreateAsync(Office office);
    Task UpdateAsync(string id, Office office);
    Task DeleteAsync(string id);
}