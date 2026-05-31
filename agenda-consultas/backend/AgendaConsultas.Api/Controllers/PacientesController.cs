using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaConsultas.Api.Controllers;

[ApiController]
[Route("api/pacientes")]
[Authorize]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _service;

    public PacientesController(IPacienteService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todos os pacientes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Paciente>>> GetAll()
    {
        var pacientes = await _service.GetAllAsync();
        return Ok(pacientes);
    }

    /// <summary>
    /// Busca um paciente pelo id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Paciente>> GetById(string id)
    {
        try
        {
            var paciente = await _service.GetByIdAsync(id);
            return Ok(paciente);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cria um novo paciente.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// {
    ///   "nome": "Ana Silva",
    ///   "cpf": "12345678901",
    ///   "telefone": "11999999999",
    ///   "email": "ana@email.com"
    /// }
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<Paciente>> Create(PacienteCreateDto dto)
    {
        try
        {
            var paciente = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um paciente por id (admin).
    /// </summary>
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

    /// <summary>
    /// Remove um paciente por id (admin).
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
