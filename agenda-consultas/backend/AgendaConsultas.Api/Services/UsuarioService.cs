using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
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
        return usuarios.Select(MapToListDto).ToList();
    }

    public async Task<UsuarioListDto> GetByIdAsync(string id)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return MapToListDto(usuario);
    }

    public async Task UpdateAsync(string id, UsuarioUpdateDto dto)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        if (!IsValidRole(dto.Role))
        {
            throw new ArgumentException("Role must be admin or usuario");
        }

        var existingByEmail = await _repository.GetByEmailAsync(email);
        if (existingByEmail is not null && existingByEmail.Id != id)
        {
            throw new ArgumentException("Email already in use");
        }

        usuario.Email = email;
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim());
        usuario.Role = dto.Role.Trim().ToLowerInvariant();

        await _repository.UpdateAsync(usuario);
    }

    public async Task PatchAsync(string id, UsuarioPatchDto dto)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        string? email = null;
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            email = dto.Email.Trim().ToLowerInvariant();
            var existingByEmail = await _repository.GetByEmailAsync(email);
            if (existingByEmail is not null && existingByEmail.Id != id)
            {
                throw new ArgumentException("Email already in use");
            }
        }

        string? passwordHash = null;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var trimmedPassword = dto.Password.Trim();
            if (trimmedPassword.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters");
            }

            passwordHash = BCrypt.Net.BCrypt.HashPassword(trimmedPassword);
        }

        string? role = null;
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            role = dto.Role.Trim().ToLowerInvariant();
            if (!IsValidRole(role))
            {
                throw new ArgumentException("Role must be admin or usuario");
            }
        }

        await _repository.PatchAsync(id, email, passwordHash, role);
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

    private static UsuarioListDto MapToListDto(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Email = usuario.Email,
        Role = usuario.Role
    };

    private static bool IsValidRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        var normalized = role.Trim().ToLowerInvariant();
        return normalized is "admin" or "usuario";
    }
}
