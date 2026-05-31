using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgendaConsultas.Api.Models;

public class Consulta
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("pacienteId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PacienteId { get; set; } = string.Empty;

    [BsonElement("data")]
    public DateTime Data { get; set; }

    [BsonElement("dataBrasil")]
    public string? DataBrasil { get; set; }

    [BsonElement("especialidade")]
    public string Especialidade { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "agendada";
}
