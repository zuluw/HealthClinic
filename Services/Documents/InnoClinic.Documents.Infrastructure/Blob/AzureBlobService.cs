using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InnoClinic.Documents.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Documents.Infrastructure.Blob
{
    public class AzureBlobService : IBlobService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobService> _logger;

        public AzureBlobService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<AzureBlobService> logger)
        {
            _logger = logger;
            var containerName = configuration["BlobStorage:ContainerName"];
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
        {
            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            _logger.LogInformation("Uploading file to blob storage: {BlobName}", blobName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(fileStream, options, ct);

            return blobName;
        }

        public async Task<(Stream Content, string ContentType)> DownloadAsync(string blobName, CancellationToken ct)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            _logger.LogInformation("Downloading file from blob storage: {BlobName}", blobName);

            var response = await blobClient.DownloadStreamingAsync(null, ct);

            return (response.Value.Content, response.Value.Details.ContentType);
        }

        public async Task DeleteAsync(string blobName, CancellationToken ct)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            _logger.LogInformation("Deleting file from blob storage: {BlobName}", blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, ct);
        }
    }
}