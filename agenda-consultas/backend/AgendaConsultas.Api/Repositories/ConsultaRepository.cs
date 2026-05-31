using AgendaConsultas.Api.Data;
using AgendaConsultas.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AgendaConsultas.Api.Repositories;

public class ConsultaRepository : IConsultaRepository
{
    private readonly IMongoCollection<Consulta> _collection;

    public ConsultaRepository(MongoDbContext context)
    {
        _collection = context.Consultas;
    }

    public Task<List<Consulta>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public async Task<Consulta?> GetByIdAsync(string id) =>
        ObjectId.TryParse(id, out _) ?
            await _collection.Find(c => c.Id == id).FirstOrDefaultAsync() :
            null;

    public Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId) =>
        _collection.Find(c => c.PacienteId == pacienteId).ToListAsync();

    public Task<List<Consulta>> GetByDateRangeAsync(DateTime start, DateTime end) =>
        _collection.Find(c => c.Data >= start && c.Data < end).ToListAsync();

    public Task CreateAsync(Consulta consulta) =>
        _collection.InsertOneAsync(consulta);

    public Task UpdateAsync(string id, Consulta consulta) =>
        ObjectId.TryParse(id, out _) ?
            _collection.ReplaceOneAsync(c => c.Id == id, consulta) :
            Task.CompletedTask;

    public Task DeleteAsync(string id) =>
        ObjectId.TryParse(id, out _) ?
            _collection.DeleteOneAsync(c => c.Id == id) :
            Task.CompletedTask;

    public async Task<bool> ExistsAsync(string id) =>
        ObjectId.TryParse(id, out _) && await _collection.Find(c => c.Id == id).AnyAsync();

    public async Task<bool> ExistsAtAsync(DateTime data, string? ignoreId = null)
    {
        if (!string.IsNullOrWhiteSpace(ignoreId))
        {
            return await _collection
                .Find(c => c.Data == data && c.Status != "cancelada" && c.Id != ignoreId)
                .AnyAsync();
        }

        return await _collection
            .Find(c => c.Data == data && c.Status != "cancelada")
            .AnyAsync();
    }
}
