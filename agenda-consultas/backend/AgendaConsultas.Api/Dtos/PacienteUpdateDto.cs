using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

public class PacienteUpdateDto
{
    [Required]
    [MinLength(3)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MinLength(11)]
    [MaxLength(14)]
    public string Cpf { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Telefone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
