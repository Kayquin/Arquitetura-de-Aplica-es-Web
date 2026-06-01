using AgendaConsultas.Api.Dtos;

namespace AgendaConsultas.Api.Services;

// Contrato administrativo para usuarios.
public interface IUsuarioService
{
    // Lista usuarios de forma simplificada.
    Task<List<UsuarioListDto>> GetAllAsync();
    // Remove usuario por id.
    Task DeleteAsync(string id);
}
