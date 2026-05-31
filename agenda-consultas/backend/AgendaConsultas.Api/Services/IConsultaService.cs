using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Services;

public interface IConsultaService
{
    Task<List<Consulta>> GetAllAsync();
    Task<Consulta> GetByIdAsync(string id);
    Task<List<Consulta>> GetByPacienteIdAsync(string pacienteId);
    Task<List<ConsultaSlotDto>> GetSlotsAsync(DateTime date);
    Task<Consulta> CreateAsync(ConsultaCreateDto dto);
    Task UpdateAsync(string id, ConsultaUpdateDto dto);
    Task DeleteAsync(string id);
}
