using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace AgendaConsultas.Api.Controllers;

// CRUD de consultas com regras de visibilidade por role.
[ApiController]
[Route("api/consultas")]
[Authorize]
public class ConsultasController : ControllerBase
{
    private readonly IConsultaService _service;
    private readonly IPacienteRepository _pacienteRepository;

    public ConsultasController(IConsultaService service, IPacienteRepository pacienteRepository)
    {
        _service = service;
        _pacienteRepository = pacienteRepository;
    }

    /// <summary>
    /// Lista todas as consultas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Consulta>>> GetAll()
    {
        // Admin ve todas as consultas.
        if (User.IsInRole("admin"))
        {
            var consultas = await _service.GetAllAsync();
            return Ok(consultas);
        }

        // Usuario comum ve apenas consultas do seu email.
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

    /// <summary>
    /// Busca uma consulta pelo id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Consulta>> GetById(string id)
    {
        try
        {
            var consulta = await _service.GetByIdAsync(id);
            // Admin acessa livremente.
            if (User.IsInRole("admin"))
            {
                return Ok(consulta);
            }

            // Usuario comum precisa ser dono da consulta.
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

    /// <summary>
    /// Lista consultas por paciente.
    /// </summary>
    [HttpGet("paciente/{pacienteId}")]
    public async Task<ActionResult<List<Consulta>>> GetByPacienteId(string pacienteId)
    {
        // Usuario comum so consulta o proprio paciente.
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

    /// <summary>
    /// Lista horarios padronizados e disponibilidade por dia.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("slots")]
    public async Task<ActionResult<List<ConsultaSlotDto>>> GetSlots([FromQuery] string? date)
    {
        // Date opcional: quando vazio, usa a data de hoje.
        var dateValue = string.IsNullOrWhiteSpace(date)
            ? DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : date;

        // Valida formato yyyy-MM-dd para evitar parsing ambiguo.
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

    /// <summary>
    /// Cria uma nova consulta.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// {
    ///   "pacienteId": "65f1b9a2a3b5c6d7e8f9a010",
    ///   "data": "2026-05-27T14:00:00Z",
    ///   "especialidade": "clinico",
    ///   "status": "agendada"
    /// }
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<Consulta>> Create(ConsultaCreateDto dto)
    {
        try
        {
            // Service valida horario e conflito.
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

    /// <summary>
    /// Atualiza uma consulta por id (admin).
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, ConsultaUpdateDto dto)
    {
        try
        {
            // Atualizacao de consulta e restrita a admin.
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

    /// <summary>
    /// Remove uma consulta por id (admin).
    /// </summary>
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            // Remocao de consulta e restrita a admin.
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
