using System.ComponentModel.DataAnnotations;

namespace AgendaConsultas.Api.Dtos;

public class AuthRoleUpdateDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
