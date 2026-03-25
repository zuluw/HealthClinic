using InnoClinic.Services.Domain.Enums;

namespace InnoClinic.Services.Application.DTOs
{
    public record UpdateServiceRequestDTO(string ServiceName, decimal Price, bool IsActive, ServiceCategory Category);
}
