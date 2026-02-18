using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Application.Interfaces;
using InnoClinic.Offices.Domain.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InnoClinic.Offices.Infrastructure.Repositories
{
    public class OfficeRepository(IConfiguration configuration) : IOfficeRepository
    {
        private readonly IMongoCollection<Office> _offices =
            new MongoClient(configuration["MongoDbSettings:ConnectionString"])
                .GetDatabase(configuration["MongoDbSettings:DatabaseName"])
                .GetCollection<Office>("Offices");

        public async Task<(IEnumerable<Office> Items, long TotalCount)> GetAllAsync(PaginationParameters parameters, CancellationToken ct)
        {
            var filter = Builders<Office>.Filter.Empty;

            var totalCount = await _offices.CountDocumentsAsync(filter, cancellationToken: ct);

            var items = await _offices.Find(filter)
                .Skip((parameters.PageNumber - 1) * PaginationParameters.PageSize)
                .Limit(PaginationParameters.PageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Office> GetByIdAsync(string id, CancellationToken ct)
        {
            return await _offices.Find(o => o.Id == id).FirstOrDefaultAsync(ct);
        }

        public async Task CreateAsync(Office office, CancellationToken ct)
        {
            await _offices.InsertOneAsync(office, cancellationToken: ct);
        }

        public async Task UpdateAsync(string id, Office office, CancellationToken ct)
        {
            await _offices.ReplaceOneAsync(o => o.Id == id, office, cancellationToken: ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct)
        {
            await _offices.DeleteOneAsync(o => o.Id == id, ct);
        }
    }
}