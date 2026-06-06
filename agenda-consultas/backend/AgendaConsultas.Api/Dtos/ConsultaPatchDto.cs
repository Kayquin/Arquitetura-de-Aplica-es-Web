namespace AgendaConsultas.Api.Dtos;

// Dados de entrada para atualização parcial de consulta (PATCH).
// Apenas os campos informados serao atualizados.
public class ConsultaPatchDto
{
    // Nova data/hora da consulta (horario Brasil). Null = nao alterar.
    public DateTime? Data { get; set; }

    // Nova especialidade medica. Null ou vazio = nao alterar.
    public string? Especialidade { get; set; }

    // Novo status da consulta (agendada, concluida, cancelada). Null = nao alterar.
    public string? Status { get; set; }
}
