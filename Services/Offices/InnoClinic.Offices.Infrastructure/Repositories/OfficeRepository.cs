using InnoClinic.Offices.Application.Interfaces;
using InnoClinic.Offices.Domain.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InnoClinic.Offices.Infrastructure.Repositories;

public class OfficeRepository : IOfficeRepository
{
    private readonly IMongoCollection<Office> _offices;

    public OfficeRepository(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDbSettings:ConnectionString"]);
        var database = client.GetDatabase(configuration["MongoDbSettings:DatabaseName"]);

        _offices = database.GetCollection<Office>("Offices");
    }

    public async Task<IEnumerable<Office>> GetAllAsync()
    {
        return await _offices.Find(_ => true).ToListAsync();
    }

    public async Task<Office?> GetByIdAsync(string id)
    {
        return await _offices.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Office office)
    {
        await _offices.InsertOneAsync(office);
    }

    public async Task UpdateAsync(string id, Office office)
    {
        await _offices.ReplaceOneAsync(o => o.Id == id, office);
    }

    public async Task DeleteAsync(string id)
    {
        await _offices.DeleteOneAsync(o => o.Id == id);
    }
}