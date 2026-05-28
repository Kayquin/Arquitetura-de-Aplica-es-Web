using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Services;

public interface IPacienteService
{
    Task<List<Paciente>> GetAllAsync();
    Task<Paciente> GetByIdAsync(string id);
    Task<Paciente> CreateAsync(PacienteCreateDto dto);
    Task UpdateAsync(string id, PacienteUpdateDto dto);
    Task DeleteAsync(string id);
}
