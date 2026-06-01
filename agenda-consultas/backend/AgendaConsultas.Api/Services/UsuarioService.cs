using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Repositories;

namespace AgendaConsultas.Api.Services;

// Regras administrativas para usuarios.
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UsuarioListDto>> GetAllAsync()
    {
        // Mapeia modelo interno para DTO enxuto.
        var usuarios = await _repository.GetAllAsync();
        return usuarios.Select(usuario => new UsuarioListDto
        {
            Id = usuario.Id,
            Email = usuario.Email,
            Role = usuario.Role
        }).ToList();
    }

    public async Task DeleteAsync(string id)
    {
        // Garante existencia antes de remover.
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        await _repository.DeleteAsync(id);
    }
}
