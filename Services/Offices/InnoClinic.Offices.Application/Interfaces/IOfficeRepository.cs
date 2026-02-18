using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Domain.Models;

namespace InnoClinic.Offices.Application.Interfaces
{
    public interface IOfficeRepository
    {
        Task<(IEnumerable<Office> Items, long TotalCount)> GetAllAsync(PaginationParameters parameters, CancellationToken ct);
        Task<Office> GetByIdAsync(string id, CancellationToken ct);
        Task CreateAsync(Office office, CancellationToken ct);
        Task UpdateAsync(string id, Office office, CancellationToken ct);
        Task DeleteAsync(string id, CancellationToken ct);
    }
}