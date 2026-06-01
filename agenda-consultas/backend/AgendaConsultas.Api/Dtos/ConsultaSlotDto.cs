namespace AgendaConsultas.Api.Dtos;

// Representa um horario padrao e sua disponibilidade.
public class ConsultaSlotDto
{
    // Data/hora do slot no horario do Brasil.
    public DateTime Data { get; set; }
    // Indica se o horario esta livre.
    public bool Disponivel { get; set; }
}
