namespace AgendaConsultas.Api.Dtos;

// Resposta da atualizacao de role.
public class AuthRoleUpdateResponseDto
{
    // Email atualizado.
    public string Email { get; set; } = string.Empty;
    // Role atual.
    public string Role { get; set; } = string.Empty;
}
