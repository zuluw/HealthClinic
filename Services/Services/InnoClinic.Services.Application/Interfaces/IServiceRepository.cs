using InnoClinic.Services.Domain.Enums;
using InnoClinic.Services.Domain.Models;

namespace InnoClinic.Services.Application.Interfaces
{
    public interface IServiceRepository
    {
        Task<List<Service>> GetByCategoryAsync(ServiceCategory category, CancellationToken ct);
        Task<List<Specialization>> GetSpecializationsWithServicesAsync(ServiceCategory category, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}