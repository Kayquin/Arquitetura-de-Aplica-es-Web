using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgendaConsultas.Api.Models;

// Entidade de consulta armazenada no MongoDB.
public class Consulta
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    // Id do documento no MongoDB.
    public string Id { get; set; } = string.Empty;

    [BsonElement("pacienteId")]
    [BsonRepresentation(BsonType.ObjectId)]
    // Referencia para o paciente.
    public string PacienteId { get; set; } = string.Empty;

    [BsonElement("data")]
    // Data em UTC, usada para regras e consultas.
    public DateTime Data { get; set; }

    [BsonElement("dataBrasil")]
    // Data formatada com offset -03:00 para exibicao.
    public string? DataBrasil { get; set; }

    [BsonElement("especialidade")]
    // Especialidade medica da consulta.
    public string Especialidade { get; set; } = string.Empty;

    [BsonElement("status")]
    // Status atual (agendada, concluida, cancelada).
    public string Status { get; set; } = "agendada";
}
