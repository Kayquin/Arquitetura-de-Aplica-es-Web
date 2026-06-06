namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para atualização parcial de usuario (PATCH).
// Apenas os campos informados serao atualizados.
public class UsuarioPatchDto
{
    // Novo email. Null ou vazio = nao alterar.
    public string? Email { get; set; }

    // Nova senha em texto plano. Null ou vazio = nao alterar.
    public string? Password { get; set; }

    // Novo role (admin/usuario). Null ou vazio = nao alterar.
    public string? Role { get; set; }
}
