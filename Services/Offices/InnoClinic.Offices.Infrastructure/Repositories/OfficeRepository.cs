using InnoClinic.Offices.Application.Interfaces;
using InnoClinic.Offices.Domain.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InnoClinic.Offices.Infrastructure.Repositories;

public class OfficeRepository(IConfiguration configuration) : IOfficeRepository
{
    private readonly IMongoCollection<Office> _offices = 
        new MongoClient(configuration["MongoDbSettings:ConnectionString"])
            .GetDatabase(configuration["MongoDbSettings:DatabaseName"])
            .GetCollection<Office>("Offices");

    public async Task<IEnumerable<Office>> GetAllAsync()
    {
        return await _offices.Find(_ => true).ToListAsync();
    }

    public async Task<Office> GetByIdAsync(string id)
    {
        return await _offices.Find(o => o.Id == id).FirstAsync();
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