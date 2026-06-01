namespace AgendaConsultas.Api.Dtos;

// Resposta de autenticacao com token e dados basicos.
public class AuthResponseDto
{
    // Token JWT para uso no header Authorization.
    public string Token { get; set; } = string.Empty;
    // Email autenticado.
    public string Email { get; set; } = string.Empty;
    // Role atual do usuario.
    public string Role { get; set; } = string.Empty;
}
