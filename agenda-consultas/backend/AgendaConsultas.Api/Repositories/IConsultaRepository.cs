using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Repositories;

// Contrato de persistencia para consultas.
public interface IConsultaRepository
{
    // Lista todas as consultas.
    Task<List<Consulta>> GetAllAsync();
    // Busca consulta por id.
    Task<Consulta?> GetByIdAsync(string id);
    // Lista consultas por paciente.
    Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId);
    // Lista consultas em uma janela de tempo.
    Task<List<Consulta>> GetByDateRangeAsync(DateTime start, DateTime end);
    // Cria nova consulta.
    Task CreateAsync(Consulta consulta);
    // Atualiza consulta existente.
    Task UpdateAsync(string id, Consulta consulta);
    // Remove consulta por id.
    Task DeleteAsync(string id);
    // Verifica existencia por id.
    Task<bool> ExistsAsync(string id);
    // Verifica conflito de horario, com opcao de ignorar id.
    Task<bool> ExistsAtAsync(DateTime data, string? ignoreId = null);
}
