using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Repositories;

public interface IConsultaRepository
{
    Task<List<Consulta>> GetAllAsync();
    Task<Consulta?> GetByIdAsync(string id);
    Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId);
    Task<List<Consulta>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task CreateAsync(Consulta consulta);
    Task UpdateAsync(string id, Consulta consulta);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<bool> ExistsAtAsync(DateTime data, string? ignoreId = null);
}
