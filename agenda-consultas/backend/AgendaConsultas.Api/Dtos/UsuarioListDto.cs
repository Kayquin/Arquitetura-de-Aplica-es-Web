namespace AgendaConsultas.Api.Dtos;

// DTO enxuto para listagem administrativa de usuarios.
public class UsuarioListDto
{
    // Id do usuario.
    public string Id { get; set; } = string.Empty;
    // Email do usuario.
    public string Email { get; set; } = string.Empty;
    // Role atual.
    public string Role { get; set; } = string.Empty;
}
