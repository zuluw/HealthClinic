using Microsoft.AspNetCore.Http;

namespace InnoClinic.Appointments.Application.Interfaces
{
    public interface IDocumentsClient
    {
        Task<Guid> UploadDocumentAsync(IFormFile file, CancellationToken ct);
        Task DeleteDocumentAsync(Guid documentId, CancellationToken ct);
    }
}