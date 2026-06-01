using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using MongoDB.Bson;

namespace AgendaConsultas.Api.Services;

// Regras de negocio para pacientes.
public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;

    public PacienteService(IPacienteRepository repository, IUsuarioRepository usuarioRepository)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
    }

    public Task<List<Paciente>> GetAllAsync() => _repository.GetAllAsync();

    public async Task<Paciente> GetByIdAsync(string id)
    {
        // Valida existencia do paciente.
        var paciente = await _repository.GetByIdAsync(id);
        if (paciente is null)
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        return paciente;
    }

    public Task<Paciente?> GetByEmailAsync(string email) =>
        _repository.GetByEmailAsync(email.Trim());

    public async Task<Paciente> CreateAsync(PacienteCreateDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        // Validacoes de campos e unicidade.
        // Valida e garante email/cpf unicos.
        ValidatePaciente(dto.Nome, dto.Cpf, dto.Telefone, normalizedEmail);
        await ValidateUsuarioVinculadoAsync(normalizedEmail);

        if (await _repository.EmailExistsAsync(normalizedEmail))
        {
            throw new ArgumentException("Email already registered");
        }

        if (await _repository.CpfExistsAsync(dto.Cpf))
        {
            throw new ArgumentException("Cpf already registered");
        }

        var paciente = new Paciente
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Nome = dto.Nome.Trim(),
            Cpf = dto.Cpf.Trim(),
            Telefone = dto.Telefone.Trim(),
            Email = normalizedEmail
        };

        await _repository.CreateAsync(paciente);
        return paciente;
    }

    public async Task UpdateAsync(string id, PacienteUpdateDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        // Valida e garante existencia antes de atualizar.
        // Rejeita update para id inexistente.
        ValidatePaciente(dto.Nome, dto.Cpf, dto.Telefone, normalizedEmail);
        await ValidateUsuarioVinculadoAsync(normalizedEmail);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        var paciente = new Paciente
        {
            Id = existing.Id,
            Nome = dto.Nome.Trim(),
            Cpf = dto.Cpf.Trim(),
            Telefone = dto.Telefone.Trim(),
            Email = normalizedEmail
        };

        await _repository.UpdateAsync(id, paciente);
    }

    public async Task DeleteAsync(string id)
    {
        // Bloqueia delete quando o id nao existe.
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        await _repository.DeleteAsync(id);
    }

    private static void ValidatePaciente(string nome, string cpf, string telefone, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome is required");
        }

        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ArgumentException("Cpf is required");
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new ArgumentException("Telefone is required");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("Email is invalid");
        }
    }

    private async Task ValidateUsuarioVinculadoAsync(string email)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        if (usuario is null)
        {
            throw new ArgumentException("Linked user not found for provided email");
        }
    }
}
