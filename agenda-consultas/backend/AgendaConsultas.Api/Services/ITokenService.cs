using AgendaConsultas.Api.Models;

namespace AgendaConsultas.Api.Services;

public interface ITokenService
{
    string CreateToken(Usuario usuario);
}
