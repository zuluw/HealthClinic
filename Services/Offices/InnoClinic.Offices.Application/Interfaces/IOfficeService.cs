using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Domain.Models;

namespace InnoClinic.Offices.Application.Interfaces
{
    public interface IOfficeService
    {
        Task<PagedResult<Office>> GetAllAsync(PaginationParameters parameters, CancellationToken ct);
        Task<Office?> GetByIdAsync(string id, CancellationToken ct);
        Task<Office> CreateAsync(OfficeDto officeDto, CancellationToken ct);
        Task UpdateAsync(string id, OfficeDto officeDto, CancellationToken ct);
        Task DeleteAsync(string id, CancellationToken ct);
    }
}