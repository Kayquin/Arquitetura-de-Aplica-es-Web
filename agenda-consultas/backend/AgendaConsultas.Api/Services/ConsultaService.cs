using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using MongoDB.Bson;
using System.Linq;

namespace AgendaConsultas.Api.Services;

public class ConsultaService : IConsultaService
{
    private readonly IConsultaRepository _consultaRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private static readonly TimeZoneInfo BrazilTimeZone = GetBrazilTimeZone();
    private static readonly TimeSpan[] HorariosPadrao =
    {
        TimeSpan.FromHours(8),
        TimeSpan.FromHours(9),
        TimeSpan.FromHours(10),
        TimeSpan.FromHours(11),
        TimeSpan.FromHours(13),
        TimeSpan.FromHours(14),
        TimeSpan.FromHours(15),
        TimeSpan.FromHours(16),
        TimeSpan.FromHours(17)
    };

    public ConsultaService(IConsultaRepository consultaRepository, IPacienteRepository pacienteRepository)
    {
        _consultaRepository = consultaRepository;
        _pacienteRepository = pacienteRepository;
    }

    public Task<List<Consulta>> GetAllAsync() => _consultaRepository.GetAllAsync();

    public Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId) =>
        _consultaRepository.GetByPacienteIdAsync(pacienteId);

    public async Task<List<ConsultaSlotDto>> GetSlotsAsync(DateTime date)
    {
        var startBrazil = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
        var endBrazil = startBrazil.AddDays(1);
        var consultasDoDia = await _consultaRepository.GetByDateRangeAsync(
            ToUtcFromBrazil(startBrazil),
            ToUtcFromBrazil(endBrazil));
        var ocupados = new HashSet<TimeSpan>(
            consultasDoDia
                .Where(c => !IsCancelada(c.Status))
                .Select(c => ToBrazilTime(c.Data).TimeOfDay));

        return HorariosPadrao
            .Select(horario => new ConsultaSlotDto
            {
                Data = startBrazil.Add(horario),
                Disponivel = !ocupados.Contains(horario)
            })
            .ToList();
    }

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
        // Garante paciente existente antes de criar consulta.
        ValidateConsulta(dto.PacienteId, dto.Data, dto.Especialidade);

        if (!IsHorarioPadrao(dto.Data))
        {
            throw new ArgumentException("Horario invalido. Use horarios padronizados.");
        }

        var dataUtc = ToUtcFromBrazil(dto.Data);

        if (!await _pacienteRepository.ExistsAsync(dto.PacienteId))
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        var status = NormalizeStatus(dto.Status);
        if (!IsCancelada(status) && await _consultaRepository.ExistsAtAsync(dataUtc))
        {
            throw new ArgumentException("Horario indisponivel");
        }

        var consulta = new Consulta
        {
            Id = ObjectId.GenerateNewId().ToString(),
            PacienteId = dto.PacienteId.Trim(),
            Data = dataUtc,
            DataBrasil = FormatBrazilOffset(dto.Data),
            Especialidade = dto.Especialidade.Trim(),
            Status = status
        };

        await _consultaRepository.CreateAsync(consulta);
        return consulta;
    }

    public async Task UpdateAsync(string id, ConsultaUpdateDto dto)
    {
        // Bloqueia update se consulta ou paciente nao existir.
        ValidateConsulta(dto.PacienteId, dto.Data, dto.Especialidade);

        var existing = await _consultaRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new KeyNotFoundException("Consulta not found");
        }

        var dataUtc = ToUtcFromBrazil(dto.Data);

        if (!IsHorarioPadrao(dto.Data) && dataUtc != existing.Data)
        {
            throw new ArgumentException("Horario invalido. Use horarios padronizados.");
        }

        if (!await _pacienteRepository.ExistsAsync(dto.PacienteId))
        {
            throw new KeyNotFoundException("Paciente not found");
        }

        var status = NormalizeStatus(dto.Status);
        if (!IsCancelada(status) && await _consultaRepository.ExistsAtAsync(dataUtc, existing.Id))
        {
            throw new ArgumentException("Horario indisponivel");
        }

        var consulta = new Consulta
        {
            Id = existing.Id,
            PacienteId = dto.PacienteId.Trim(),
            Data = dataUtc,
            DataBrasil = FormatBrazilOffset(dto.Data),
            Especialidade = dto.Especialidade.Trim(),
            Status = status
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

    private static bool IsHorarioPadrao(DateTime data) =>
        HorariosPadrao.Contains(ToBrazilTime(data).TimeOfDay);

    private static string NormalizeStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "agendada" : status.Trim().ToLowerInvariant();

    private static bool IsCancelada(string? status) =>
        string.Equals(status, "cancelada", StringComparison.OrdinalIgnoreCase);

    private static DateTime ToBrazilTime(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(value, BrazilTimeZone);
        }

        if (value.Kind == DateTimeKind.Local)
        {
            return TimeZoneInfo.ConvertTime(value, BrazilTimeZone);
        }

        return value;
    }

    private static DateTime ToUtcFromBrazil(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return value;
        }

        var brazil = value.Kind == DateTimeKind.Unspecified
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(brazil, BrazilTimeZone);
    }

    private static TimeZoneInfo GetBrazilTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    }

    private static string FormatBrazilOffset(DateTime value)
    {
        var brazil = value.Kind == DateTimeKind.Unspecified
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        var offset = BrazilTimeZone.GetUtcOffset(brazil);
        var dto = new DateTimeOffset(brazil, offset);
        return dto.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }
}
