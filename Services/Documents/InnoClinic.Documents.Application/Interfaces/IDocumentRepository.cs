using InnoClinic.Documents.Domain.Models;

namespace InnoClinic.Documents.Application.Interfaces
{
    public interface IDocumentRepository
    {
        void Add(Document document);
        Task<Document?> GetByIdAsync(Guid id, CancellationToken ct);
        void Delete(Document document);
        Task SaveChangesAsync(CancellationToken ct);
    }
}