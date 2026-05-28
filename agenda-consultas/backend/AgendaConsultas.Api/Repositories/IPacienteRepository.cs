using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Repositories;

public interface IPacienteRepository
{
    Task<List<Paciente>> GetAllAsync();
    Task<Paciente?> GetByIdAsync(string id);
    Task CreateAsync(Paciente paciente);
    Task UpdateAsync(string id, Paciente paciente);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CpfExistsAsync(string cpf);
}
