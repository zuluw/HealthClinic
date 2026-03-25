using InnoClinic.Services.Domain.Enums;

namespace InnoClinic.Services.Application.DTOs
{
    public record ServiceResponseDTO(Guid Id, string ServiceName, decimal Price, bool IsActive, ServiceCategory Category);
}
