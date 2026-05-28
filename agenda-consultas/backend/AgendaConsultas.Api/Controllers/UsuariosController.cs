using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaConsultas.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "admin")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
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
