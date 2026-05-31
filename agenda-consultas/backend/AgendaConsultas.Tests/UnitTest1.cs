using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using AgendaConsultas.Api.Services;

namespace AgendaConsultas.Tests;

public class PacienteServiceTests
{
    [Fact]
    public async Task CreatePaciente_ReturnsPaciente()
    {
        var repo = new InMemoryPacienteRepository();
        var service = new PacienteService(repo);

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        var result = await service.CreateAsync(dto);

        Assert.Equal(dto.Email, result.Email);
        Assert.Single(await repo.GetAllAsync());
    }

    [Fact]
    public async Task CreatePaciente_InvalidEmail_Throws()
    {
        var repo = new InMemoryPacienteRepository();
        var service = new PacienteService(repo);

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "invalid"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreatePaciente_DuplicateEmail_Throws()
    {
        var repo = new InMemoryPacienteRepository();
        var service = new PacienteService(repo);

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        await service.CreateAsync(dto);

        var dtoDuplicado = new PacienteCreateDto
        {
            Nome = "Carlos Lima",
            Cpf = "98765432100",
            Telefone = "21999999999",
            Email = "ana@email.com"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dtoDuplicado));
    }

    [Fact]
    public async Task CreatePaciente_DuplicateCpf_Throws()
    {
        var repo = new InMemoryPacienteRepository();
        var service = new PacienteService(repo);

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        await service.CreateAsync(dto);

        var dtoDuplicado = new PacienteCreateDto
        {
            Nome = "Carlos Lima",
            Cpf = "12345678901",
            Telefone = "21999999999",
            Email = "carlos@email.com"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dtoDuplicado));
    }

    [Fact]
    public async Task UpdatePaciente_NotFound_Throws()
    {
        var repo = new InMemoryPacienteRepository();
        var service = new PacienteService(repo);

        var dto = new PacienteUpdateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync("missing", dto));
    }

    [Fact]
    public async Task DeletePaciente_NotFound_Throws()
    {
        var repo = new InMemoryPacienteRepository();
        var service = new PacienteService(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync("missing"));
    }

    [Fact]
    public async Task UpdatePaciente_UpdatesData()
    {
        var repo = new InMemoryPacienteRepository();
        var service = new PacienteService(repo);

        var paciente = new Paciente
        {
            Id = "paciente1",
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        await repo.CreateAsync(paciente);

        var dto = new PacienteUpdateDto
        {
            Nome = "Ana Maria",
            Cpf = "12345678901",
            Telefone = "11888888888",
            Email = "ana@email.com"
        };

        await service.UpdateAsync("paciente1", dto);

        var updated = await repo.GetByIdAsync("paciente1");
        Assert.NotNull(updated);
        Assert.Equal("Ana Maria", updated!.Nome);
        Assert.Equal("11888888888", updated.Telefone);
    }
}

public class ConsultaServiceTests
{
    [Fact]
    public async Task CreateConsulta_ReturnsConsulta()
    {
        var (service, consultaRepo, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);
        var dto = new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        };

        var result = await service.CreateAsync(dto);

        Assert.Equal(dto.PacienteId, result.PacienteId);
        Assert.Single(await consultaRepo.GetAllAsync());
    }

    [Fact]
    public async Task CreateConsulta_PacienteMissing_Throws()
    {
        var (service, _, _) = CreateConsultaService();
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);
        var dto = new ConsultaCreateDto
        {
            PacienteId = "missing",
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateConsulta_InvalidHorario_Throws()
    {
        var (service, _, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 12);
        var dto = new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateConsulta_HorarioOcupado_Throws()
    {
        var (service, consultaRepo, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);

        await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        });

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "ortopedia",
            Status = "agendada"
        }));

        Assert.Single(await consultaRepo.GetAllAsync());
    }

    [Fact]
    public async Task CreateConsulta_CanceladaPermiteMesmoHorario()
    {
        var (service, consultaRepo, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 9);

        await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "cancelada"
        });

        var result = await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "cardio",
            Status = "agendada"
        });

        Assert.Equal("agendada", result.Status);
        Assert.Equal(2, (await consultaRepo.GetAllAsync()).Count);
    }

    [Fact]
    public async Task CreateConsulta_SetsDataBrasil()
    {
        var (service, _, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);

        var result = await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        });

        Assert.False(string.IsNullOrWhiteSpace(result.DataBrasil));
        Assert.Contains("2026-06-01T08:00:00", result.DataBrasil);
        Assert.EndsWith("-03:00", result.DataBrasil);
    }

    [Fact]
    public async Task GetSlotsAsync_MarcaHorarioOcupado()
    {
        var (service, _, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);

        await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        });

        var slots = await service.GetSlotsAsync(new DateTime(2026, 6, 1));
        Assert.Equal(9, slots.Count);
        Assert.Contains(slots, slot => slot.Data.TimeOfDay == TimeSpan.FromHours(8) && !slot.Disponivel);
    }

    private static (ConsultaService service, InMemoryConsultaRepository consultaRepo, InMemoryPacienteRepository pacienteRepo)
        CreateConsultaService()
    {
        var pacienteRepo = new InMemoryPacienteRepository();
        var consultaRepo = new InMemoryConsultaRepository();
        var service = new ConsultaService(consultaRepo, pacienteRepo);
        return (service, consultaRepo, pacienteRepo);
    }

    private static async Task<Paciente> CreatePacienteAsync(InMemoryPacienteRepository repo)
    {
        var paciente = new Paciente
        {
            Id = "paciente1",
            Nome = "Bruno Lima",
            Cpf = "98765432100",
            Telefone = "21999999999",
            Email = "bruno@email.com"
        };

        await repo.CreateAsync(paciente);
        return paciente;
    }

    private static DateTime BuildBrazilDate(int year, int month, int day, int hour) =>
        DateTime.SpecifyKind(new DateTime(year, month, day, hour, 0, 0), DateTimeKind.Unspecified);
}

internal class InMemoryPacienteRepository : IPacienteRepository
{
    private readonly List<Paciente> _items = new();

    public Task<List<Paciente>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<Paciente?> GetByIdAsync(string id) =>
        Task.FromResult(_items.FirstOrDefault(p => p.Id == id));

    public Task<Paciente?> GetByEmailAsync(string email) =>
        Task.FromResult(_items.FirstOrDefault(p =>
            string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task CreateAsync(Paciente paciente)
    {
        _items.Add(paciente);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(string id, Paciente paciente)
    {
        var index = _items.FindIndex(p => p.Id == id);
        if (index >= 0)
        {
            _items[index] = paciente;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _items.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id) =>
        Task.FromResult(_items.Any(p => p.Id == id));

    public Task<bool> EmailExistsAsync(string email) =>
        Task.FromResult(_items.Any(p => p.Email == email));

    public Task<bool> CpfExistsAsync(string cpf) =>
        Task.FromResult(_items.Any(p => p.Cpf == cpf));
}

internal class InMemoryConsultaRepository : IConsultaRepository
{
    private readonly List<Consulta> _items = new();

    public Task<List<Consulta>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<Consulta?> GetByIdAsync(string id) =>
        Task.FromResult(_items.FirstOrDefault(c => c.Id == id));

    public Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId) =>
        Task.FromResult(_items.Where(c => c.PacienteId == pacienteId).ToList());

    public Task<List<Consulta>> GetByDateRangeAsync(DateTime start, DateTime end) =>
        Task.FromResult(_items.Where(c => c.Data >= start && c.Data < end).ToList());

    public Task CreateAsync(Consulta consulta)
    {
        _items.Add(consulta);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(string id, Consulta consulta)
    {
        var index = _items.FindIndex(c => c.Id == id);
        if (index >= 0)
        {
            _items[index] = consulta;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _items.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id) =>
        Task.FromResult(_items.Any(c => c.Id == id));

    public Task<bool> ExistsAtAsync(DateTime data, string? ignoreId = null) =>
        Task.FromResult(_items.Any(c =>
            c.Data == data &&
            c.Status != "cancelada" &&
            (string.IsNullOrWhiteSpace(ignoreId) || c.Id != ignoreId)));
}
