using InnoClinic.Documents.Application.DTOs;
using InnoClinic.Documents.Application.Interfaces;
using InnoClinic.Documents.Application.Mappings;
using InnoClinic.Documents.Domain.Exceptions;
using InnoClinic.Documents.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Documents.Application.Services
{
    public class DocumentService(IBlobService blobService, IDocumentRepository repository, DocumentMapper mapper, ILogger<DocumentService> logger) : IDocumentService
    {
        public async Task<DocumentResponseDTO> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
        {
            logger.LogInformation("Starting upload process for file: {FileName}", fileName);

            var blobName = await blobService.UploadAsync(fileStream, fileName, contentType, ct);

            var document = new Document
            {
                FileName = fileName,
                ContentType = contentType,
                BlobName = blobName
            };

            repository.Add(document);
            await repository.SaveChangesAsync(ct);

            logger.LogInformation("File {FileName} successfully uploaded and registered with ID: {Id}", fileName, document.Id);

            return mapper.ToDto(document);
        }

        public async Task<(Stream Content, string ContentType, string FileName)> DownloadAsync(Guid id, CancellationToken ct)
        {
            var document = await repository.GetByIdAsync(id, ct);
            if (document == null)
            {
                throw new NotFoundException("Document not found");
            }

            var (content, contentType) = await blobService.DownloadAsync(document.BlobName, ct);

            return (content, contentType, document.FileName);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var document = await repository.GetByIdAsync(id, ct);
            if (document == null) throw new NotFoundException("Document not found");

            await blobService.DeleteAsync(document.BlobName, ct);

            repository.Delete(document);
            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Document {Id} deleted from storage and database", id);
        }
    }
}