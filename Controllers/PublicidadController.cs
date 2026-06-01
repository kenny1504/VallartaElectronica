using Microsoft.AspNetCore.Mvc;

namespace ElectronicaVallarta.Controllers;

public class PublicidadController : Controller
{
    [HttpGet("/publicidad")]
    public IActionResult Index()
    {
        return View();
    }
}
