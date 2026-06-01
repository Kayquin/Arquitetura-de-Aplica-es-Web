using AgendaConsultas.Api.Dtos;
using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Services;

// Contrato de negocio para pacientes.
public interface IPacienteService
{
    // Lista todos os pacientes.
    Task<List<Paciente>> GetAllAsync();
    // Busca paciente por id.
    Task<Paciente> GetByIdAsync(string id);
    // Cria paciente com validacoes.
    Task<Paciente> CreateAsync(PacienteCreateDto dto);
    // Atualiza paciente por id.
    Task UpdateAsync(string id, PacienteUpdateDto dto);
    // Remove paciente por id.
    Task DeleteAsync(string id);
}
