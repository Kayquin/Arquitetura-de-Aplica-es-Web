using AgendaConsultas.Api.Data;
using AgendaConsultas.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AgendaConsultas.Api.Repositories;

// Acesso aos dados de consultas no MongoDB.
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
        // Evita erro quando o id nao e ObjectId valido.
        ObjectId.TryParse(id, out _) ?
            await _collection.Find(c => c.Id == id).FirstOrDefaultAsync() :
            null;

    public Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId) =>
        _collection.Find(c => c.PacienteId == pacienteId).ToListAsync();

    public Task<List<Consulta>> GetByDateRangeAsync(DateTime start, DateTime end) =>
        // Filtro por janela de datas (UTC).
        _collection.Find(c => c.Data >= start && c.Data < end).ToListAsync();

    public Task CreateAsync(Consulta consulta) =>
        _collection.InsertOneAsync(consulta);

    public Task UpdateAsync(string id, Consulta consulta) =>
        // ReplaceOne so ocorre quando o id e valido.
        ObjectId.TryParse(id, out _) ?
            _collection.ReplaceOneAsync(c => c.Id == id, consulta) :
            Task.CompletedTask;

    public Task DeleteAsync(string id) =>
        // Delete silencioso se id invalido.
        ObjectId.TryParse(id, out _) ?
            _collection.DeleteOneAsync(c => c.Id == id) :
            Task.CompletedTask;

    public async Task<bool> ExistsAsync(string id) =>
        ObjectId.TryParse(id, out _) && await _collection.Find(c => c.Id == id).AnyAsync();

    public async Task<bool> ExistsAtAsync(DateTime data, string? ignoreId = null)
    {
        // Verifica conflito de horario ignorando consulta especifica.
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
