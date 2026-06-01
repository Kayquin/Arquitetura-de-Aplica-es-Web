using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgendaConsultas.Api.Models;

// Entidade de usuario autenticado.
public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    // Id do documento no MongoDB.
    public string Id { get; set; } = string.Empty;

    [BsonElement("email")]
    // Email usado para login.
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    // Hash BCrypt da senha.
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    // Perfil do usuario (admin/usuario).
    public string Role { get; set; } = "usuario";
}
