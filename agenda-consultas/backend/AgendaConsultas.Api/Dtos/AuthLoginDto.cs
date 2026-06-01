using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para login.
public class AuthLoginDto
{
    [Required]
    [EmailAddress]
    // Email do usuario.
    public string Email { get; set; } = string.Empty;

    [Required]
    // Senha em texto puro.
    public string Password { get; set; } = string.Empty;
}
