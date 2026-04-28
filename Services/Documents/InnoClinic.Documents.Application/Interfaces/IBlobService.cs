namespace InnoClinic.Documents.Application.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct);
        Task<(Stream Content, string ContentType)> DownloadAsync(string blobName, CancellationToken ct);
        Task DeleteAsync(string blobName, CancellationToken ct);
    }
}