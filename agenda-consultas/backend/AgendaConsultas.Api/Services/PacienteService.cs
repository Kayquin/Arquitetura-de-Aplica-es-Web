using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using MongoDB.Bson;

namespace AgendaConsultas.Api.Services;

public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _repository;

    public PacienteService(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Paciente>> GetAllAsync() => _repository.GetAllAsync();

    public async Task<Paciente> GetByIdAsync(string id)
    {
        var paciente = await _repository.GetByIdAsync(id);
        if (paciente is null)
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        return paciente;
    }

    public async Task<Paciente> CreateAsync(PacienteCreateDto dto)
    {
        // Valida e garante email/cpf unicos.
        ValidatePaciente(dto.Nome, dto.Cpf, dto.Telefone, dto.Email);

        if (await _repository.EmailExistsAsync(dto.Email))
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
            Email = dto.Email.Trim()
        };

        await _repository.CreateAsync(paciente);
        return paciente;
    }

    public async Task UpdateAsync(string id, PacienteUpdateDto dto)
    {
        // Rejeita update para id inexistente.
        ValidatePaciente(dto.Nome, dto.Cpf, dto.Telefone, dto.Email);

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
            Email = dto.Email.Trim()
        };

        await _repository.UpdateAsync(id, paciente);
    }

    public async Task DeleteAsync(string id)
    {
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
}
