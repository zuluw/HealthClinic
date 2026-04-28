using InnoClinic.Documents.Application.Interfaces;
using InnoClinic.Documents.Domain.Models;
using InnoClinic.Documents.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Documents.Infrastructure.Repositories
{
    public class DocumentRepository(AppDbContext context) : IDocumentRepository
    {
        public void Add(Document document) 
        {
            context.Documents.Add(document);
        }

        public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await context.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public void Delete(Document document) 
        {
            context.Documents.Remove(document);
        }

        public async Task SaveChangesAsync(CancellationToken ct) 
        {
            await context.SaveChangesAsync(ct);
        }
    }
}