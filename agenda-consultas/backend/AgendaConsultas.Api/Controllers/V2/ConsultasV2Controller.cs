using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace AgendaConsultas.Api.Controllers.V2;

// CRUD de consultas com regras de visibilidade por role (v2).
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/consultas")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class ConsultasV2Controller : ControllerBase
{
    private readonly IConsultaService _service;
    private readonly IPacienteRepository _pacienteRepository;

    public ConsultasV2Controller(IConsultaService service, IPacienteRepository pacienteRepository)
    {
        _service = service;
        _pacienteRepository = pacienteRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Consulta>>> GetAll()
    {
        if (User.IsInRole("admin"))
        {
            var consultas = await _service.GetAllAsync();
            return Ok(consultas);
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var paciente = await _pacienteRepository.GetByEmailAsync(email);
        if (paciente is null)
        {
            return Ok(Array.Empty<Consulta>());
        }

        var consultasDoPaciente = await _service.GetByPacienteIdAsync(paciente.Id);
        return Ok(consultasDoPaciente);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Consulta>> GetById(string id)
    {
        try
        {
            var consulta = await _service.GetByIdAsync(id);
            if (User.IsInRole("admin"))
            {
                return Ok(consulta);
            }

            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var paciente = await _pacienteRepository.GetByIdAsync(consulta.PacienteId);
            if (paciente is null || !string.Equals(paciente.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return Ok(consulta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("paciente/{pacienteId}")]
    public async Task<ActionResult<List<Consulta>>> GetByPacienteId(string pacienteId)
    {
        if (!User.IsInRole("admin"))
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            var paciente = await _pacienteRepository.GetByIdAsync(pacienteId);
            if (paciente is null || !string.Equals(paciente.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
        }

        var consultas = await _service.GetByPacienteIdAsync(pacienteId);
        return Ok(consultas);
    }

    [AllowAnonymous]
    [HttpGet("slots")]
    public async Task<ActionResult<List<ConsultaSlotDto>>> GetSlots([FromQuery] string? date)
    {
        var dateValue = string.IsNullOrWhiteSpace(date)
            ? DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : date;

        if (!DateTime.TryParseExact(
                dateValue,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return BadRequest(new { message = "Data invalida. Use yyyy-MM-dd" });
        }

        var slots = await _service.GetSlotsAsync(parsed);
        return Ok(slots);
    }

    [HttpPost]
    public async Task<ActionResult<Consulta>> Create(ConsultaCreateDto dto)
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

                var paciente = await _pacienteRepository.GetByIdAsync(dto.PacienteId);
                if (paciente is null)
                {
                    return NotFound(new { message = "Paciente not found" });
                }

                if (!string.Equals(paciente.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            var consulta = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = consulta.Id }, consulta);
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

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, ConsultaUpdateDto dto)
    {
        try
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
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

    [Authorize(Roles = "admin")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(string id, ConsultaPatchDto dto)
    {
        try
        {
            await _service.PatchAsync(id, dto);
            return NoContent();
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
