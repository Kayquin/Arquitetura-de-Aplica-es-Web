using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para registro.
public class AuthRegisterDto
{
    [Required]
    [EmailAddress]
    // Email do usuario.
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    // Senha em texto puro (sera hash). 
    public string Password { get; set; } = string.Empty;

    // Role inicial (usuario/admin).
    public string Role { get; set; } = "usuario";

    // Dados do paciente criados automaticamente para role usuario.
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}
