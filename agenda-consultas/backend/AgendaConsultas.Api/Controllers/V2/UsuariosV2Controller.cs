using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaConsultas.Api.Controllers.V2;

// Endpoints administrativos de usuarios (v2) com operacoes ampliadas.
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/usuarios")]
[Authorize(Roles = "admin")]
[ApiExplorerSettings(GroupName = "v2")]
public class UsuariosV2Controller : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosV2Controller(IUsuarioService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todos os usuarios (admin).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UsuarioListDto>>> GetAll()
    {
        var usuarios = await _service.GetAllAsync();
        return Ok(usuarios);
    }

    /// <summary>
    /// Busca um usuario pelo id (admin).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioListDto>> GetById(string id)
    {
        try
        {
            var usuario = await _service.GetByIdAsync(id);
            return Ok(usuario);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um usuario por completo (admin).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UsuarioUpdateDto dto)
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
    /// Atualiza parcialmente um usuario (admin).
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(string id, UsuarioPatchDto dto)
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

    /// <summary>
    /// Remove um usuario por id (admin).
    /// </summary>
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
