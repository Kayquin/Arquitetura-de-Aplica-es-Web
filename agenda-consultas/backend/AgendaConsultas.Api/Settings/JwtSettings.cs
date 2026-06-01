namespace AgendaConsultas.Api.Settings;

// Mapeia configuracoes da secao Jwt em appsettings.json.
public class JwtSettings
{
    // Chave simetrica usada para assinar tokens.
    public string Key { get; set; } = string.Empty;
    // Emissor esperado no token.
    public string Issuer { get; set; } = string.Empty;
    // Audiencia esperada no token.
    public string Audience { get; set; } = string.Empty;
    // Duracao do token em minutos.
    public int ExpirationMinutes { get; set; } = 60;
}
