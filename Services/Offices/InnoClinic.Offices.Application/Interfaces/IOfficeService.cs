using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Domain.Models;

namespace InnoClinic.Offices.Application.Interfaces
{
    public interface IOfficeService
    {
        Task<PagedResult<OfficeResponseDTO>> GetAllAsync(PaginationParameters parameters, CancellationToken ct);
        Task<OfficeResponseDTO?> GetByIdAsync(string id, CancellationToken ct);
        Task<OfficeResponseDTO> CreateAsync(OfficeRequestDTO officeDto, CancellationToken ct);
        Task UpdateAsync(string id, OfficeRequestDTO officeDto, CancellationToken ct);
        Task DeleteAsync(string id, CancellationToken ct);
    }
}