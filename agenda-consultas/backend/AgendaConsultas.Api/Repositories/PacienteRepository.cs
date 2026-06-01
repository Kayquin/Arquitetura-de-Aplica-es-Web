using AgendaConsultas.Api.Data;
using AgendaConsultas.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AgendaConsultas.Api.Repositories;

// Acesso aos dados de pacientes no MongoDB.
public class PacienteRepository : IPacienteRepository
{
    private readonly IMongoCollection<Paciente> _collection;

    public PacienteRepository(MongoDbContext context)
    {
        _collection = context.Pacientes;
    }

    public Task<List<Paciente>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public async Task<Paciente?> GetByIdAsync(string id) =>
        // Evita erro quando o id nao e ObjectId valido.
        ObjectId.TryParse(id, out _) ?
            await _collection.Find(p => p.Id == id).FirstOrDefaultAsync() :
            null;

    public async Task<Paciente?> GetByEmailAsync(string email) =>
        // Busca direta por email.
        await _collection.Find(p => p.Email == email).FirstOrDefaultAsync();

    public Task CreateAsync(Paciente paciente) =>
        _collection.InsertOneAsync(paciente);

    public Task UpdateAsync(string id, Paciente paciente) =>
        // ReplaceOne so ocorre quando o id e valido.
        ObjectId.TryParse(id, out _) ?
            _collection.ReplaceOneAsync(p => p.Id == id, paciente) :
            Task.CompletedTask;

    public Task DeleteAsync(string id) =>
        // Delete silencioso se id invalido.
        ObjectId.TryParse(id, out _) ?
            _collection.DeleteOneAsync(p => p.Id == id) :
            Task.CompletedTask;

    public async Task<bool> ExistsAsync(string id) =>
        ObjectId.TryParse(id, out _) && await _collection.Find(p => p.Id == id).AnyAsync();

    public async Task<bool> EmailExistsAsync(string email) =>
        await _collection.Find(p => p.Email == email).AnyAsync();

    public async Task<bool> CpfExistsAsync(string cpf) =>
        await _collection.Find(p => p.Cpf == cpf).AnyAsync();
}
