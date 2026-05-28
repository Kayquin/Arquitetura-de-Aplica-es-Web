using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgendaConsultas.Api.Models;

public class Paciente
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("nome")]
    public string Nome { get; set; } = string.Empty;

    [BsonElement("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [BsonElement("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;
}
