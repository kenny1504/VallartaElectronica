using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicaVallarta.Controllers;

public class CuentaController(IServicioAutenticacionAdministrador servicioAutenticacionAdministrador) : Controller
{
    [AllowAnonymous]
    [HttpGet("/acceso")]
    public IActionResult Ingresar(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Administracion");
        }

        return View(new FormularioInicioSesionViewModel { UrlRetorno = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("/acceso")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingresar(FormularioInicioSesionViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await servicioAutenticacionAdministrador.ValidarCredencialesAsync(modelo);
        if (!resultado.EsValido || resultado.Principal is null)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(modelo);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, resultado.Principal);

        if (!string.IsNullOrWhiteSpace(modelo.UrlRetorno) && Url.IsLocalUrl(modelo.UrlRetorno))
        {
            return Redirect(modelo.UrlRetorno);
        }

        return RedirectToAction("Index", "Administracion");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
