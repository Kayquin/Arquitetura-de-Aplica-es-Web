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

    public Task UpdateAsync(Usuario usuario)
    {
        // Atualiza documento completo por id.
        return _collection.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);
    }

    public Task PatchAsync(string id, string? email, string? passwordHash, string? role)
    {
        var updates = new List<UpdateDefinition<Usuario>>();

        if (!string.IsNullOrWhiteSpace(email))
        {
            updates.Add(Builders<Usuario>.Update.Set(u => u.Email, email));
        }

        if (!string.IsNullOrWhiteSpace(passwordHash))
        {
            updates.Add(Builders<Usuario>.Update.Set(u => u.PasswordHash, passwordHash));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            updates.Add(Builders<Usuario>.Update.Set(u => u.Role, role));
        }

        if (updates.Count == 0)
        {
            return Task.CompletedTask;
        }

        var combined = Builders<Usuario>.Update.Combine(updates);
        return _collection.UpdateOneAsync(u => u.Id == id, combined);
    }

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
