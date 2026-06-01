using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Repositories;

// Contrato de persistencia para pacientes.
public interface IPacienteRepository
{
    // Lista todos os pacientes.
    Task<List<Paciente>> GetAllAsync();
    // Busca por id.
    Task<Paciente?> GetByIdAsync(string id);
    // Busca por email.
    Task<Paciente?> GetByEmailAsync(string email);
    // Cria novo paciente.
    Task CreateAsync(Paciente paciente);
    // Atualiza paciente existente.
    Task UpdateAsync(string id, Paciente paciente);
    // Remove paciente por id.
    Task DeleteAsync(string id);
    // Verifica existencia por id.
    Task<bool> ExistsAsync(string id);
    // Verifica duplicidade de email.
    Task<bool> EmailExistsAsync(string email);
    // Verifica duplicidade de CPF.
    Task<bool> CpfExistsAsync(string cpf);
}
