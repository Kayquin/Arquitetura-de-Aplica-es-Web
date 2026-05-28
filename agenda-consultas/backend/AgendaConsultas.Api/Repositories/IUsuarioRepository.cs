using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Repositories;

public interface IUsuarioRepository
{
    Task<List<Usuario>> GetAllAsync();
    Task<Usuario?> GetByIdAsync(string id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task CreateAsync(Usuario usuario);
    Task UpdateRoleAsync(string email, string role);
    Task DeleteAsync(string id);
}
