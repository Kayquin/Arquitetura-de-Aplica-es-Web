using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using MongoDB.Bson;

namespace AgendaConsultas.Api.Services;

public class ConsultaService : IConsultaService
{
    private readonly IConsultaRepository _consultaRepository;
    private readonly IPacienteRepository _pacienteRepository;

    public ConsultaService(IConsultaRepository consultaRepository, IPacienteRepository pacienteRepository)
    {
        _consultaRepository = consultaRepository;
        _pacienteRepository = pacienteRepository;
    }

    public Task<List<Consulta>> GetAllAsync() => _consultaRepository.GetAllAsync();

    public Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId) =>
        _consultaRepository.GetByPacienteIdAsync(pacienteId);

    public async Task<Consulta> GetByIdAsync(string id)
    {
        var consulta = await _consultaRepository.GetByIdAsync(id);
        if (consulta is null)
        {
            throw new KeyNotFoundException("Consulta not found");
        }

        return consulta;
    }

    public async Task<Consulta> CreateAsync(ConsultaCreateDto dto)
    {
        ValidateConsulta(dto.PacienteId, dto.Data, dto.Especialidade);

        if (!await _pacienteRepository.ExistsAsync(dto.PacienteId))
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        var consulta = new Consulta
        {
            Id = ObjectId.GenerateNewId().ToString(),
            PacienteId = dto.PacienteId.Trim(),
            Data = dto.Data,
            Especialidade = dto.Especialidade.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "agendada" : dto.Status.Trim()
        };

        await _consultaRepository.CreateAsync(consulta);
        return consulta;
    }

    public async Task UpdateAsync(string id, ConsultaUpdateDto dto)
    {
        ValidateConsulta(dto.PacienteId, dto.Data, dto.Especialidade);

        var existing = await _consultaRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new KeyNotFoundException("Consulta not found");
        }

        if (!await _pacienteRepository.ExistsAsync(dto.PacienteId))
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        var consulta = new Consulta
        {
            Id = existing.Id,
            PacienteId = dto.PacienteId.Trim(),
            Data = dto.Data,
            Especialidade = dto.Especialidade.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "agendada" : dto.Status.Trim()
        };

        await _consultaRepository.UpdateAsync(id, consulta);
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _consultaRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new KeyNotFoundException("Consulta not found");
        }

        await _consultaRepository.DeleteAsync(id);
    }

    private static void ValidateConsulta(string pacienteId, DateTime data, string especialidade)
    {
        if (string.IsNullOrWhiteSpace(pacienteId))
        {
            throw new ArgumentException("PacienteId is required");
        }

        if (data == default)
        {
            throw new ArgumentException("Data is required");
        }

        if (string.IsNullOrWhiteSpace(especialidade))
        {
            throw new ArgumentException("Especialidade is required");
        }
    }
}
