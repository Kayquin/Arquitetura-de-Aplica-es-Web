using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;

namespace AgendaConsultas.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUsuarioRepository usuarioRepository, ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(AuthRegisterDto dto)
    {
        // Validacao basica do cadastro.
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
        {
            throw new ArgumentException("Email is invalid");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
        {
            throw new ArgumentException("Password is invalid");
        }

        var existing = await _usuarioRepository.GetByEmailAsync(dto.Email.Trim());
        if (existing is not null)
        {
            throw new ArgumentException("Email already registered");
        }

        // Restringe roles permitidos.
        var role = string.IsNullOrWhiteSpace(dto.Role) ? "usuario" : dto.Role.Trim().ToLowerInvariant();
        if (role != "admin" && role != "usuario")
        {
            throw new ArgumentException("Role is invalid");
        }

        // Hash da senha antes de salvar.
        var usuario = new Usuario
        {
            Email = dto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role
        };

        await _usuarioRepository.CreateAsync(usuario);

        return new AuthResponseDto
        {
            Email = usuario.Email,
            Role = usuario.Role,
            Token = _tokenService.CreateToken(usuario)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(AuthLoginDto dto)
    {
        // Valida credenciais e emite token.
        var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email.Trim());
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        return new AuthResponseDto
        {
            Email = usuario.Email,
            Role = usuario.Role,
            Token = _tokenService.CreateToken(usuario)
        };
    }

    public async Task<AuthRoleUpdateResponseDto> UpdateRoleAsync(AuthRoleUpdateDto dto)
    {
        // Fluxo de alteracao de role (admin).
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
        {
            throw new ArgumentException("Email is invalid");
        }

        var role = string.IsNullOrWhiteSpace(dto.Role) ? string.Empty : dto.Role.Trim().ToLowerInvariant();
        if (role != "admin" && role != "usuario")
        {
            throw new ArgumentException("Role is invalid");
        }

        var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email.Trim());
        if (usuario is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        await _usuarioRepository.UpdateRoleAsync(usuario.Email, role);

        return new AuthRoleUpdateResponseDto
        {
            Email = usuario.Email,
            Role = role
        };
    }
}
