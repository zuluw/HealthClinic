using InnoClinic.Documents.Application.DTOs;

namespace InnoClinic.Documents.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponseDTO> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct);
        Task<(Stream Content, string ContentType, string FileName)> DownloadAsync(Guid id, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}