using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicaVallarta.Controllers;

[Authorize(Roles = "Administrador")]
public class AdministracionController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
