using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Services;

// Contrato de negocio para consultas e horarios.
public interface IConsultaService
{
    // Lista todas as consultas.
    Task<List<Consulta>> GetAllAsync();
    // Busca consulta por id.
    Task<Consulta> GetByIdAsync(string id);
    // Lista consultas de um paciente.
    Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId);
    // Lista horarios padronizados e disponibilidade do dia.
    Task<List<ConsultaSlotDto>> GetSlotsAsync(DateTime date);
    // Cria consulta com validacoes.
    Task<Consulta> CreateAsync(ConsultaCreateDto dto);
    // Atualiza consulta por id.
    Task UpdateAsync(string id, ConsultaUpdateDto dto);
    // Remove consulta por id.
    Task DeleteAsync(string id);
}
