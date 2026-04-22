using System.Net.Http.Json;
using InnoClinic.Appointments.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace InnoClinic.Appointments.Infrastructure.ExternalServices
{
    public class DocumentsClient(HttpClient httpClient, ILogger<DocumentsClient> logger) : IDocumentsClient
    {
        private record UploadResponse(Guid Id);

        public async Task<Guid> UploadDocumentAsync(IFormFile file, CancellationToken ct)
        {
            logger.LogInformation("Forwarding file {FileName} to Documents API", file.FileName);

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await httpClient.PostAsync("api/documents/upload", content, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken: ct);
            return result!.Id;
        }

        public async Task DeleteDocumentAsync(Guid documentId, CancellationToken ct)
        {
            logger.LogInformation("Requesting deletion of document {Id} from Documents API", documentId);
            var response = await httpClient.DeleteAsync($"api/documents/{documentId}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}