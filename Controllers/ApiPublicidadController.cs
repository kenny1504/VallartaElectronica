using ElectronicaVallarta.Interfaces.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicaVallarta.Controllers;

[ApiController]
[Route("api/publicidad")]
public class ApiPublicidadController(IServicioPublicidad servicioPublicidad) : ControllerBase
{
    [HttpGet("activa")]
    [HttpGet("activam")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ObtenerActiva()
    {
        var publicidades = await servicioPublicidad.ObtenerPublicidadesActivasAsync(DateTime.Now);
        return Ok(publicidades);
    }
}
