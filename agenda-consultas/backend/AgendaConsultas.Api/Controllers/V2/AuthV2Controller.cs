using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaConsultas.Api.Controllers.V2;

// Endpoints de autenticacao e gerenciamento de roles (v2).
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiExplorerSettings(GroupName = "v2")]
public class AuthV2Controller : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthV2Controller(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(AuthRegisterDto dto)
    {
        try
        {
            var response = await _authService.RegisterAsync(dto);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(AuthLoginDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("role")]
    public async Task<ActionResult<AuthRoleUpdateResponseDto>> UpdateRole(AuthRoleUpdateDto dto)
    {
        try
        {
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
