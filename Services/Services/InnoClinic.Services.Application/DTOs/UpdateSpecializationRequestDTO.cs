namespace InnoClinic.Services.Application.DTOs
{
    public record UpdateSpecializationRequestDTO(string Name, bool IsActive, List<UpdateServiceItemRequestDTO> Services);
}
