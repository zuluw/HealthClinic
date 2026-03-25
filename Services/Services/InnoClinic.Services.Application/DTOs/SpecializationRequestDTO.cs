namespace InnoClinic.Services.Application.DTOs
{
    public record SpecializationRequestDTO(string Name, bool IsActive, List<ServiceRequestDTO> Services);
}