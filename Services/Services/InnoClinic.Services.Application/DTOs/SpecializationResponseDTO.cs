namespace InnoClinic.Services.Application.DTOs
{
    public record SpecializationResponseDTO(string SpecializationName, List<ServiceResponseDTO> Services);
}
