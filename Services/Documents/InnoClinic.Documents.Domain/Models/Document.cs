namespace InnoClinic.Documents.Domain.Models
{
    public class Document
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string FileName { get; set; }      
        public required string ContentType { get; set; }   
        public required string BlobName { get; set; }      
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}