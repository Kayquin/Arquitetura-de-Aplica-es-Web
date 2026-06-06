using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para atualizar usuario por completo.
public class UsuarioUpdateDto
{
    [Required]
    [EmailAddress]
    // Novo email do usuario.
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    // Nova senha em texto plano (sera armazenada com hash).
    public string Password { get; set; } = string.Empty;

    [Required]
    // Novo role (admin/usuario).
    public string Role { get; set; } = string.Empty;
}
