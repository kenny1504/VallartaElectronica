using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.Controllers;

[Authorize(Roles = "Administrador")]
public class AdministracionSucursalesController(IServicioSucursal servicioSucursal, IServicioPais servicioPais) : Controller
{
    public async Task<IActionResult> Index() => View(await servicioSucursal.ObtenerSucursalesAsync());

    public async Task<IActionResult> Crear() => View(await ConstruirFormularioAsync(new FormularioSucursalViewModel { EstaActivo = true }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(FormularioSucursalViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await ConstruirFormularioAsync(modelo));

        try
        {
            await servicioSucursal.CrearAsync(new Sucursal { PaisId = modelo.PaisId!.Value, Nombre = modelo.Nombre, EstaActivo = modelo.EstaActivo });
            TempData["MensajeExito"] = "Sucursal creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await ConstruirFormularioAsync(modelo));
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        var sucursal = await servicioSucursal.ObtenerSucursalPorIdAsync(id);
        if (sucursal is null) return NotFound();

        return View(await ConstruirFormularioAsync(new FormularioSucursalViewModel
        {
            Id = sucursal.Id,
            PaisId = sucursal.PaisId,
            Nombre = sucursal.Nombre,
            EstaActivo = sucursal.EstaActivo
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(FormularioSucursalViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await ConstruirFormularioAsync(modelo));

        try
        {
            await servicioSucursal.ActualizarAsync(new Sucursal { Id = modelo.Id, PaisId = modelo.PaisId!.Value, Nombre = modelo.Nombre, EstaActivo = modelo.EstaActivo });
            TempData["MensajeExito"] = "Sucursal actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await ConstruirFormularioAsync(modelo));
        }
    }

    public async Task<IActionResult> Eliminar(int id)
    {
        var sucursal = await servicioSucursal.ObtenerSucursalPorIdAsync(id);
        return sucursal is null ? NotFound() : View(sucursal);
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        try
        {
            await servicioSucursal.EliminarAsync(id);
            TempData["MensajeExito"] = "Sucursal eliminada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = $"No se pudo eliminar la sucursal: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<FormularioSucursalViewModel> ConstruirFormularioAsync(FormularioSucursalViewModel modelo)
    {
        var paises = await servicioPais.ObtenerPaisesActivosAsync();
        modelo.Paises = paises.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
        return modelo;
    }
}
