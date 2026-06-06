using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgendaConsultas.Api.Controllers.V2;

// CRUD de pacientes com protecao por JWT (v2).
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/pacientes")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class PacientesV2Controller : ControllerBase
{
    private readonly IPacienteService _service;
    private readonly IUsuarioRepository _usuarioRepository;

    public PacientesV2Controller(IPacienteService service, IUsuarioRepository usuarioRepository)
    {
        _service = service;
        _usuarioRepository = usuarioRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Paciente>>> GetAll()
    {
        if (!User.IsInRole("admin"))
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var proprioPaciente = await _service.GetByEmailAsync(email);
            return Ok(proprioPaciente is null ? Array.Empty<Paciente>() : new[] { proprioPaciente });
        }

        var pacientes = await _service.GetAllAsync();
        return Ok(pacientes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Paciente>> GetById(string id)
    {
        try
        {
            var paciente = await _service.GetByIdAsync(id);

            if (!User.IsInRole("admin"))
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Unauthorized();
                }

                if (!string.Equals(paciente.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            return Ok(paciente);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Paciente>> Create(PacienteCreateDto dto)
    {
        try
        {
            if (!User.IsInRole("admin"))
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Unauthorized();
                }

                dto.Email = email;
            }
            else
            {
                var email = dto.Email.Trim().ToLowerInvariant();
                var existingUser = await _usuarioRepository.GetByEmailAsync(email);
                if (existingUser is null)
                {
                    if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Trim().Length < 6)
                    {
                        return BadRequest(new { message = "Informe senha (min 6) para criar usuario junto com o paciente" });
                    }

                    await _usuarioRepository.CreateAsync(new Usuario
                    {
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim()),
                        Role = "usuario"
                    });
                }

                dto.Email = email;
            }

            var paciente = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, PacienteUpdateDto dto)
    {
        try
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(string id, PacientePatchDto dto)
    {
        try
        {
            await _service.PatchAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
