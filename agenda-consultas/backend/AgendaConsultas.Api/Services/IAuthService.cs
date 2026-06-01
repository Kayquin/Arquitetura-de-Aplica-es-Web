using AgendaConsultas.Api.Dtos;

namespace AgendaConsultas.Api.Services;

// Contrato para autenticacao e gerenciamento de roles.
public interface IAuthService
{
    // Cria usuario e retorna token.
    Task<AuthResponseDto> RegisterAsync(AuthRegisterDto dto);
    // Valida credenciais e retorna token.
    Task<AuthResponseDto> LoginAsync(AuthLoginDto dto);
    // Atualiza role de usuario.
    Task<AuthRoleUpdateResponseDto> UpdateRoleAsync(AuthRoleUpdateDto dto);
}
