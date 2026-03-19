namespace InnoClinic.Services.Application.DTOs
{
    public record SpecializationResponseDTO(Guid Id, string SpecializationName, List<ServiceResponseDTO> Services);
}
