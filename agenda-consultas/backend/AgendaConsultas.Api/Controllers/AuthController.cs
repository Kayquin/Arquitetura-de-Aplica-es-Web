using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaConsultas.Api.Controllers;

// Endpoints de autenticacao e gerenciamento de roles.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registra um novo usuario e retorna o token JWT.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// {
    ///   "email": "usuario@email.com",
    ///   "password": "123456",
    ///   "role": "usuario",
    ///   "nome": "Ana Silva",
    ///   "cpf": "12345678901",
    ///   "telefone": "11999999999"
    /// }
    /// </remarks>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(AuthRegisterDto dto)
    {
        try
        {
            // Delegado para o service que valida e cria o usuario.
            var response = await _authService.RegisterAsync(dto);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Autentica um usuario e retorna o token JWT.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// {
    ///   "email": "admin@email.com",
    ///   "password": "123456"
    /// }
    /// </remarks>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(AuthLoginDto dto)
    {
        try
        {
            // Service valida credenciais e emite JWT.
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza o role de um usuario (admin).
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// {
    ///   "email": "usuario@email.com",
    ///   "role": "admin"
    /// }
    /// </remarks>
    [Authorize(Roles = "admin")]
    [HttpPut("role")]
    public async Task<ActionResult<AuthRoleUpdateResponseDto>> UpdateRole(AuthRoleUpdateDto dto)
    {
        try
        {
            // Apenas admin pode alterar role de outros usuarios.
            var response = await _authService.UpdateRoleAsync(dto);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
