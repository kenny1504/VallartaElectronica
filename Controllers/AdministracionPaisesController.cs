using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicaVallarta.Controllers;

[Authorize(Roles = "Administrador")]
public class AdministracionPaisesController(IServicioPais servicioPais) : Controller
{
    public async Task<IActionResult> Index() => View(await servicioPais.ObtenerPaisesAsync());

    public IActionResult Crear() => View(new FormularioPaisViewModel { EstaActivo = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(FormularioPaisViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            await servicioPais.CrearAsync(new Pais
            {
                Nombre = modelo.Nombre,
                CodigoMoneda = modelo.CodigoMoneda,
                SimboloMoneda = modelo.SimboloMoneda,
                EstaActivo = modelo.EstaActivo
            });
            TempData["MensajeExito"] = "Pais creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        var pais = await servicioPais.ObtenerPaisPorIdAsync(id);
        if (pais is null) return NotFound();

        return View(new FormularioPaisViewModel
        {
            Id = pais.Id,
            Nombre = pais.Nombre,
            CodigoMoneda = pais.CodigoMoneda,
            SimboloMoneda = pais.SimboloMoneda,
            EstaActivo = pais.EstaActivo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(FormularioPaisViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            await servicioPais.ActualizarAsync(new Pais
            {
                Id = modelo.Id,
                Nombre = modelo.Nombre,
                CodigoMoneda = modelo.CodigoMoneda,
                SimboloMoneda = modelo.SimboloMoneda,
                EstaActivo = modelo.EstaActivo
            });
            TempData["MensajeExito"] = "Pais actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    public async Task<IActionResult> Eliminar(int id)
    {
        var pais = await servicioPais.ObtenerPaisPorIdAsync(id);
        return pais is null ? NotFound() : View(pais);
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        try
        {
            await servicioPais.EliminarAsync(id);
            TempData["MensajeExito"] = "Pais eliminado correctamente.";
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = $"No se pudo eliminar el pais: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
