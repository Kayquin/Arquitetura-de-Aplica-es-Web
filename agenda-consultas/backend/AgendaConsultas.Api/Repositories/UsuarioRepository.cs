using AgendaConsultas.Api.Data;
using AgendaConsultas.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AgendaConsultas.Api.Repositories;

// Acesso aos dados de usuarios no MongoDB.
public class UsuarioRepository : IUsuarioRepository
{
    private readonly IMongoCollection<Usuario> _collection;

    public UsuarioRepository(MongoDbContext context)
    {
        _collection = context.Usuarios;
    }

    public async Task<Usuario?> GetByEmailAsync(string email) =>
        await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<Usuario?> GetByIdAsync(string id) =>
        // Evita erro quando o id nao e ObjectId valido.
        ObjectId.TryParse(id, out _) ?
            await _collection.Find(u => u.Id == id).FirstOrDefaultAsync() :
            null;

    public Task<List<Usuario>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task CreateAsync(Usuario usuario) =>
        _collection.InsertOneAsync(usuario);

    public Task UpdateRoleAsync(string email, string role)
    {
        // Atualiza role por email.
        var update = Builders<Usuario>.Update.Set(u => u.Role, role);
        return _collection.UpdateOneAsync(u => u.Email == email, update);
    }

    public Task DeleteAsync(string id) =>
        // Delete silencioso se id invalido.
        ObjectId.TryParse(id, out _) ?
            _collection.DeleteOneAsync(u => u.Id == id) :
            Task.CompletedTask;
}
