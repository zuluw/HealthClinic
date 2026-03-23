namespace InnoClinic.Services.Application.DTOs
{
    public record SpecializationResponseDTO(Guid Id, string SpecializationName, bool IsActive, List<ServiceResponseDTO> Services);
}
