using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para atualizar paciente.
public class PacienteUpdateDto
{
    [Required]
    [MinLength(3)]
    // Nome completo.
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MinLength(11)]
    [MaxLength(14)]
    // CPF do paciente (com ou sem pontuacao).
    public string Cpf { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    // Telefone do paciente.
    public string Telefone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    // Email para contato.
    public string Email { get; set; } = string.Empty;
}
