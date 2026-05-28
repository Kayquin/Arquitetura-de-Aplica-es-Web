using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

public class ConsultaCreateDto
{
    [Required]
    public string PacienteId { get; set; } = string.Empty;

    [Required]
    public DateTime Data { get; set; }

    [Required]
    [MinLength(3)]
    public string Especialidade { get; set; } = string.Empty;

    public string Status { get; set; } = "agendada";
}
