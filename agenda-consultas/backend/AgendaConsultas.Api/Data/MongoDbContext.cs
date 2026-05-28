using AgendaConsultas.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AgendaConsultas.Api.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<Paciente> Pacientes => _database.GetCollection<Paciente>("pacientes");
    public IMongoCollection<Consulta> Consultas => _database.GetCollection<Consulta>("consultas");
    public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("usuarios");
}
