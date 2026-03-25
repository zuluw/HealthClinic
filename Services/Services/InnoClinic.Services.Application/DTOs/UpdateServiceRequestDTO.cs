using InnoClinic.Services.Domain.Enums;

namespace InnoClinic.Services.Application.DTOs
{
    public record UpdateServiceRequestDTO(Guid? Id, string ServiceName, decimal Price, bool IsActive, ServiceCategory Category);
}
