using AgendaConsultas.Api.Dtos;

namespace AgendaConsultas.Api.Services;

// Contrato administrativo para usuarios.
public interface IUsuarioService
{
    // Lista usuarios de forma simplificada.
    Task<List<UsuarioListDto>> GetAllAsync();
    // Busca usuario por id.
    Task<UsuarioListDto> GetByIdAsync(string id);
    // Atualiza usuario por completo.
    Task UpdateAsync(string id, UsuarioUpdateDto dto);
    // Atualiza usuario parcialmente.
    Task PatchAsync(string id, UsuarioPatchDto dto);
    // Remove usuario por id.
    Task DeleteAsync(string id);
}
