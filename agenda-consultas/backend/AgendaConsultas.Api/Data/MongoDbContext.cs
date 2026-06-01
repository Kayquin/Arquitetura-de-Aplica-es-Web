using AgendaConsultas.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AgendaConsultas.Api.Data;

// Abstrai acesso ao MongoDB e expõe colecoes tipadas.
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        // Cria client e seleciona a base configurada.
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    // Colecoes principais do dominio.
    public IMongoCollection<Paciente> Pacientes => _database.GetCollection<Paciente>("pacientes");
    public IMongoCollection<Consulta> Consultas => _database.GetCollection<Consulta>("consultas");
    public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("usuarios");
}
