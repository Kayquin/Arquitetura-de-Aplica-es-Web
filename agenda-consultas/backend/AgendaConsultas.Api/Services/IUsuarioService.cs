using AgendaConsultas.Api.Dtos;

namespace AgendaConsultas.Api.Services;

public interface IUsuarioService
{
    Task<List<UsuarioListDto>> GetAllAsync();
    Task DeleteAsync(string id);
}
