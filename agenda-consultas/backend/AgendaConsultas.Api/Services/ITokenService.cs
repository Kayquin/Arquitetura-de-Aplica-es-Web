using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Services;

// Contrato para geracao de JWT.
public interface ITokenService
{
    // Cria token com claims do usuario.
    string CreateToken(Usuario usuario);
}
