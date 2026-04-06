using System.Security.Claims;
using ElectronicaVallarta.ViewModels;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioAutenticacionAdministrador
{
    /// <summary>
    /// Valida las credenciales proporcionadas en el modelo de inicio de sesión y devuelve el resultado de la operación,
    /// incluyendo información sobre la validez de las credenciales, un mensaje asociado y un objeto <see cref="ClaimsPrincipal"/> si es exitoso.
    /// </summary>
    /// <param name="modelo">El modelo que contiene las credenciales de inicio de sesión a validar.</param>
    /// <returns>
    /// Una tupla que contiene:
    /// - <c>bool</c>: Indica si las credenciales son válidas.
    /// - <c>string</c>: Un mensaje descriptivo sobre el resultado de la validación.
    /// - <c>ClaimsPrincipal</c>: El principal de seguridad generado si la validación es exitosa, o <c>null</c> en caso contrario.
    /// </returns>
    Task<(bool EsValido, string Mensaje, ClaimsPrincipal? Principal)> ValidarCredencialesAsync(FormularioInicioSesionViewModel modelo);
}
