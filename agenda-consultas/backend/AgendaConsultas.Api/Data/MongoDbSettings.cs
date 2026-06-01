namespace AgendaConsultas.Api.Data;

// Mapeia configuracoes da secao Mongo em appsettings.json.
public class MongoDbSettings
{
    // String de conexao do MongoDB.
    public string ConnectionString { get; set; } = string.Empty;
    // Nome da base utilizada pela API.
    public string DatabaseName { get; set; } = string.Empty;
}
