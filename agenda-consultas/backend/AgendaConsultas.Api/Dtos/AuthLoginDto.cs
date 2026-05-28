using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

public class AuthLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
