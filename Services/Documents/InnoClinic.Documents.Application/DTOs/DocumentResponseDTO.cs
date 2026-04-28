namespace InnoClinic.Documents.Application.DTOs
{
    public record DocumentResponseDTO(Guid Id, string FileName, string ContentType, DateTime CreatedAt);
}