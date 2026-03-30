using System.Security.Claims;
using ElectronicaVallarta.ViewModels;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioAutenticacionAdministrador
{
    Task<(bool EsValido, string Mensaje, ClaimsPrincipal? Principal)> ValidarCredencialesAsync(FormularioInicioSesionViewModel modelo);
}
