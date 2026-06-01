using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgendaConsultas.Api.Models;

// Entidade de paciente armazenada no MongoDB.
public class Paciente
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    // Id do documento no MongoDB.
    public string Id { get; set; } = string.Empty;

    [BsonElement("nome")]
    // Nome completo do paciente.
    public string Nome { get; set; } = string.Empty;

    [BsonElement("cpf")]
    // CPF do paciente (somente digitos).
    public string Cpf { get; set; } = string.Empty;

    [BsonElement("telefone")]
    // Telefone do paciente.
    public string Telefone { get; set; } = string.Empty;

    [BsonElement("email")]
    // Email do paciente.
    public string Email { get; set; } = string.Empty;
}
