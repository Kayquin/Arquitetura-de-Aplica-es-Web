using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;
using AgendaConsultas.Api.Repositories;
using AgendaConsultas.Api.Services;

namespace AgendaConsultas.Tests;

// Testes de regras do PacienteService.
public class PacienteServiceTests
{
    [Fact]
    public async Task CreatePaciente_ReturnsPaciente()
    {
        // Arrange
        var (service, repo, _) = CreatePacienteService("ana@email.com");

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal(dto.Email, result.Email);
        Assert.Single(await repo.GetAllAsync());
    }

    [Fact]
    public async Task CreatePaciente_InvalidEmail_Throws()
    {
        // Arrange
        var (service, _, _) = CreatePacienteService();

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "invalid"
        };

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreatePaciente_DuplicateEmail_Throws()
    {
        // Arrange
        var (service, _, _) = CreatePacienteService("ana@email.com", "carlos@email.com");

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        // Act
        await service.CreateAsync(dto);

        var dtoDuplicado = new PacienteCreateDto
        {
            Nome = "Carlos Lima",
            Cpf = "98765432100",
            Telefone = "21999999999",
            Email = "ana@email.com"
        };

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dtoDuplicado));
    }

    [Fact]
    public async Task CreatePaciente_DuplicateCpf_Throws()
    {
        // Arrange
        var (service, _, _) = CreatePacienteService("ana@email.com", "carlos@email.com");

        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        // Act
        await service.CreateAsync(dto);

        var dtoDuplicado = new PacienteCreateDto
        {
            Nome = "Carlos Lima",
            Cpf = "12345678901",
            Telefone = "21999999999",
            Email = "carlos@email.com"
        };

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dtoDuplicado));
    }

    [Fact]
    public async Task UpdatePaciente_NotFound_Throws()
    {
        // Arrange
        var (service, _, _) = CreatePacienteService("ana@email.com");

        var dto = new PacienteUpdateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        // Act/Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync("missing", dto));
    }

    [Fact]
    public async Task DeletePaciente_NotFound_Throws()
    {
        // Arrange
        var (service, _, _) = CreatePacienteService();

        // Act/Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync("missing"));
    }

    [Fact]
    public async Task UpdatePaciente_UpdatesData()
    {
        // Arrange
        var (service, repo, _) = CreatePacienteService("ana@email.com");

        var paciente = new Paciente
        {
            Id = "paciente1",
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        // Act
        await repo.CreateAsync(paciente);

        var dto = new PacienteUpdateDto
        {
            Nome = "Ana Maria",
            Cpf = "12345678901",
            Telefone = "11888888888",
            Email = "ana@email.com"
        };

        // Act
        await service.UpdateAsync("paciente1", dto);

        // Assert
        var updated = await repo.GetByIdAsync("paciente1");
        Assert.NotNull(updated);
        Assert.Equal("Ana Maria", updated!.Nome);
        Assert.Equal("11888888888", updated.Telefone);
    }

    [Fact]
    public async Task CreatePaciente_LinkedUserMissing_Throws()
    {
        // Arrange
        var (service, _, _) = CreatePacienteService();
        var dto = new PacienteCreateDto
        {
            Nome = "Ana Silva",
            Cpf = "12345678901",
            Telefone = "11999999999",
            Email = "ana@email.com"
        };

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    private static (PacienteService service, InMemoryPacienteRepository pacienteRepo, InMemoryUsuarioRepository usuarioRepo)
        CreatePacienteService(params string[] userEmails)
    {
        var pacienteRepo = new InMemoryPacienteRepository();
        var usuarioRepo = new InMemoryUsuarioRepository();
        foreach (var email in userEmails)
        {
            usuarioRepo.Add(email);
        }

        var service = new PacienteService(pacienteRepo, usuarioRepo);
        return (service, pacienteRepo, usuarioRepo);
    }
}

// Testes de regras do ConsultaService.
public class ConsultaServiceTests
{
    [Fact]
    public async Task CreateConsulta_ReturnsConsulta()
    {
        // Arrange
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

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal(dto.PacienteId, result.PacienteId);
        Assert.Single(await consultaRepo.GetAllAsync());
    }

    [Fact]
    public async Task CreateConsulta_PacienteMissing_Throws()
    {
        // Arrange
        var (service, _, _) = CreateConsultaService();
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);
        var dto = new ConsultaCreateDto
        {
            PacienteId = "missing",
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        };

        // Act/Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateConsulta_InvalidHorario_Throws()
    {
        // Arrange
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

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateConsulta_HorarioOcupado_Throws()
    {
        // Arrange
        var (service, consultaRepo, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);

        // Act
        await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        });

        // Assert
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
        // Arrange
        var (service, consultaRepo, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 9);

        // Act
        await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "cancelada"
        });

        // Act
        var result = await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "cardio",
            Status = "agendada"
        });

        // Assert
        Assert.Equal("agendada", result.Status);
        Assert.Equal(2, (await consultaRepo.GetAllAsync()).Count);
    }

    [Fact]
    public async Task CreateConsulta_SetsDataBrasil()
    {
        // Arrange
        var (service, _, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);

        // Act
        var result = await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        });

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.DataBrasil));
        Assert.Contains("2026-06-01T08:00:00", result.DataBrasil);
        Assert.EndsWith("-03:00", result.DataBrasil);
    }

    [Fact]
    public async Task GetSlotsAsync_MarcaHorarioOcupado()
    {
        // Arrange
        var (service, _, pacienteRepo) = CreateConsultaService();
        var paciente = await CreatePacienteAsync(pacienteRepo);
        var dataLocal = BuildBrazilDate(2026, 6, 1, 8);

        // Act
        await service.CreateAsync(new ConsultaCreateDto
        {
            PacienteId = paciente.Id,
            Data = dataLocal,
            Especialidade = "clinico",
            Status = "agendada"
        });

        // Act
        var slots = await service.GetSlotsAsync(new DateTime(2026, 6, 1));
        // Assert
        Assert.Equal(9, slots.Count);
        Assert.Contains(slots, slot => slot.Data.TimeOfDay == TimeSpan.FromHours(8) && !slot.Disponivel);
    }

    private static (ConsultaService service, InMemoryConsultaRepository consultaRepo, InMemoryPacienteRepository pacienteRepo)
        CreateConsultaService()
    {
        // Helper para criar services com repositorios em memoria.
        var pacienteRepo = new InMemoryPacienteRepository();
        var consultaRepo = new InMemoryConsultaRepository();
        var service = new ConsultaService(consultaRepo, pacienteRepo);
        return (service, consultaRepo, pacienteRepo);
    }

    private static async Task<Paciente> CreatePacienteAsync(InMemoryPacienteRepository repo)
    {
        // Helper para criar um paciente padrao.
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
        // DateTime sem kind, tratado como horario Brasil pelo service.
        DateTime.SpecifyKind(new DateTime(year, month, day, hour, 0, 0), DateTimeKind.Unspecified);
}

// Repositorio em memoria para testes de pacientes.
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

// Repositorio em memoria para testes de consultas.
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
        // Simula regra de conflito ignorando canceladas.
        Task.FromResult(_items.Any(c =>
            c.Data == data &&
            c.Status != "cancelada" &&
            (string.IsNullOrWhiteSpace(ignoreId) || c.Id != ignoreId)));
}

internal class InMemoryUsuarioRepository : IUsuarioRepository
{
    private readonly List<Usuario> _items = new();

    public Task<List<Usuario>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<Usuario?> GetByIdAsync(string id) =>
        Task.FromResult(_items.FirstOrDefault(u => u.Id == id));

    public Task<Usuario?> GetByEmailAsync(string email) =>
        Task.FromResult(_items.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task CreateAsync(Usuario usuario)
    {
        _items.Add(usuario);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Usuario usuario)
    {
        var index = _items.FindIndex(u => u.Id == usuario.Id);
        if (index >= 0)
        {
            _items[index] = usuario;
        }
        return Task.CompletedTask;
    }

    public Task PatchAsync(string id, string? email, string? passwordHash, string? role)
    {
        var user = _items.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return Task.CompletedTask;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            user.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(passwordHash))
        {
            user.PasswordHash = passwordHash;
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            user.Role = role;
        }

        return Task.CompletedTask;
    }

    public Task UpdateRoleAsync(string email, string role)
    {
        var user = _items.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        if (user is not null)
        {
            user.Role = role;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _items.RemoveAll(u => u.Id == id);
        return Task.CompletedTask;
    }

    public void Add(string email)
    {
        _items.Add(new Usuario
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = email,
            Role = "usuario",
            PasswordHash = "hash"
        });
    }
}
