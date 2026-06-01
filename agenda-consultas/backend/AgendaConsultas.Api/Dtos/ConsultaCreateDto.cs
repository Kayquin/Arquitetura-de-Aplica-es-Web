using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para criar consulta.
public class ConsultaCreateDto
{
    [Required]
    // Id do paciente.
    public string PacienteId { get; set; } = string.Empty;

    [Required]
    // Data/hora da consulta (horario Brasil).
    public DateTime Data { get; set; }

    [Required]
    [MinLength(3)]
    // Especialidade medica.
    public string Especialidade { get; set; } = string.Empty;

    // Status inicial da consulta.
    public string Status { get; set; } = "agendada";
}
