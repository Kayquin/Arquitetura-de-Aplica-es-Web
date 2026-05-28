using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaConsultas.Api.Controllers;

[ApiController]
[Route("api/consultas")]
public class ConsultasController : ControllerBase
{
    private readonly IConsultaService _service;

    public ConsultasController(IConsultaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas as consultas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Consulta>>> GetAll()
    {
        var consultas = await _service.GetAllAsync();
        return Ok(consultas);
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
        var consultas = await _service.GetByPacienteIdAsync(pacienteId);
        return Ok(consultas);
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
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
