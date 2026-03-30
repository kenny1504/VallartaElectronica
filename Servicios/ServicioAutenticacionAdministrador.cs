using System.Security.Claims;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.ViewModels;

namespace ElectronicaVallarta.Servicios;

public class ServicioAutenticacionAdministrador(IRepositorioUsuarioAdministrador repositorioUsuarioAdministrador) : IServicioAutenticacionAdministrador
{
    public async Task<(bool EsValido, string Mensaje, ClaimsPrincipal? Principal)> ValidarCredencialesAsync(FormularioInicioSesionViewModel modelo)
    {
        var usuario = await repositorioUsuarioAdministrador.ObtenerPorNombreUsuarioAsync(modelo.NombreUsuario);
        if (usuario is null || !ServicioHashClave.VerificarHash(modelo.Clave, usuario.ClaveHash))
        {
            return (false, "Usuario o clave incorrectos.", null);
        }

        var identidad = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.GivenName, usuario.NombreCompleto),
            new Claim(ClaimTypes.Role, "Administrador")
        ], "Cookies");

        return (true, string.Empty, new ClaimsPrincipal(identidad));
    }
}
