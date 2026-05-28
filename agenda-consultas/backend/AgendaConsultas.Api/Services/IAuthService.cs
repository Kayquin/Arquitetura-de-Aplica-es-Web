using AgendaConsultas.Api.Dtos;

namespace AgendaConsultas.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(AuthRegisterDto dto);
    Task<AuthResponseDto> LoginAsync(AuthLoginDto dto);
    Task<AuthRoleUpdateResponseDto> UpdateRoleAsync(AuthRoleUpdateDto dto);
}
