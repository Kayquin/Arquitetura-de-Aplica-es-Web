using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Repositories;

// Contrato de persistencia para usuarios.
public interface IUsuarioRepository
{
    // Lista todos os usuarios.
    Task<List<Usuario>> GetAllAsync();
    // Busca por id.
    Task<Usuario?> GetByIdAsync(string id);
    // Busca por email.
    Task<Usuario?> GetByEmailAsync(string email);
    // Cria novo usuario.
    Task CreateAsync(Usuario usuario);
    // Atualiza apenas a role.
    Task UpdateRoleAsync(string email, string role);
    // Remove usuario por id.
    Task DeleteAsync(string id);
}
