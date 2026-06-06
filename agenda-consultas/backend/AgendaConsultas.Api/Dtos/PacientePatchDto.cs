namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para atualização parcial de paciente (PATCH).
// Apenas os campos informados serao atualizados.
public class PacientePatchDto
{
    // Novo nome completo. Null ou vazio = nao alterar.
    public string? Nome { get; set; }

    // Novo CPF. Null ou vazio = nao alterar.
    public string? Cpf { get; set; }

    // Novo telefone. Null ou vazio = nao alterar.
    public string? Telefone { get; set; }

    // Novo email. Null ou vazio = nao alterar.
    public string? Email { get; set; }
}
