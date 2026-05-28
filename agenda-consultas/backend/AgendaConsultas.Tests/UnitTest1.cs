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
}

public class ConsultaServiceTests
{
    [Fact]
    public async Task CreateConsulta_ReturnsConsulta()
    {
        var pacienteRepo = new InMemoryPacienteRepository();
        var consultaRepo = new InMemoryConsultaRepository();
        var service = new ConsultaService(consultaRepo, pacienteRepo);

        var paciente = new Paciente
        {
            Id = "paciente1",
            Nome = "Bruno Lima",
            Cpf = "98765432100",
            Telefone = "21999999999",
            Email = "bruno@email.com"
        };

        await pacienteRepo.CreateAsync(paciente);

        var dto = new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = DateTime.UtcNow.AddDays(1),
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
        var pacienteRepo = new InMemoryPacienteRepository();
        var consultaRepo = new InMemoryConsultaRepository();
        var service = new ConsultaService(consultaRepo, pacienteRepo);

        var dto = new ConsultaCreateDto
        {
            PacienteId = "missing",
            Data = DateTime.UtcNow.AddDays(1),
            Especialidade = "clinico",
            Status = "agendada"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(dto));
    }
}

internal class InMemoryPacienteRepository : IPacienteRepository
{
    private readonly List<Paciente> _items = new();

    public Task<List<Paciente>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<Paciente?> GetByIdAsync(string id) =>
        Task.FromResult(_items.FirstOrDefault(p => p.Id == id));

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
}
