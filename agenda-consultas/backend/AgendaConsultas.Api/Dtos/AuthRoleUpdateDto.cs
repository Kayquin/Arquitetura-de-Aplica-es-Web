using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para atualizar role.
public class AuthRoleUpdateDto
{
    [Required]
    [EmailAddress]
    // Email do usuario alvo.
    public string Email { get; set; } = string.Empty;

    [Required]
    // Novo role (admin/usuario).
    public string Role { get; set; } = string.Empty;
}
