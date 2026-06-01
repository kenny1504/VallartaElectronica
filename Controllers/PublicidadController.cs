using ElectronicaVallarta.Interfaces.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicaVallarta.Controllers;

public class PublicidadController(IServicioPublicidad servicioPublicidad) : Controller
{
    [HttpGet("/publicidad")]
    public async Task<IActionResult> Index()
    {
        var publicidades = await servicioPublicidad.ObtenerPublicidadesActivasAsync(DateTime.Now);
        return View(publicidades);
    }
}
