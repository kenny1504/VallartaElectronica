using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.Controllers;

[Authorize(Roles = "Administrador")]
public class AdministracionTasasCambioController(
    IServicioTasaCambio servicioTasaCambio,
    IServicioPais servicioPais,
    IServicioSucursal servicioSucursal) : Controller
{
    public async Task<IActionResult> Index(DateTime? fechaFiltro, bool mostrarTodos = false)
    {
        var fechaAplicada = mostrarTodos ? (DateTime?)null : (fechaFiltro ?? DateTime.Today).Date;
        var modelo = new ListadoTasasCambioViewModel
        {
            FechaFiltro = fechaAplicada,
            MostrarTodos = mostrarTodos,
            Tasas = await servicioTasaCambio.ObtenerListadoTasasAsync(fechaAplicada)
        };

        return View(modelo);
    }

    public async Task<IActionResult> Crear() => View(await ConstruirFormularioAsync(new FormularioTasaCambioViewModel { EstaActivo = true, FechaTasa = DateTime.Today }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(FormularioTasaCambioViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await ConstruirFormularioAsync(modelo));

        try
        {
            await servicioTasaCambio.CrearAsync(Mapear(modelo));
            TempData["MensajeExito"] = "Tasa creada correctamente.";
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
        var tasa = await servicioTasaCambio.ObtenerTasaPorIdAsync(id);
        if (tasa is null) return NotFound();

        return View(await ConstruirFormularioAsync(new FormularioTasaCambioViewModel
        {
            Id = tasa.Id,
            PaisId = tasa.PaisId,
            SucursalId = tasa.SucursalId,
            MontoDesdeUsd = tasa.MontoDesdeUsd,
            MontoHastaUsd = tasa.MontoHastaUsd,
            TasaCambio = tasa.TasaCambio,
            FechaTasa = tasa.FechaTasa,
            EstaActivo = tasa.EstaActivo
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(FormularioTasaCambioViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await ConstruirFormularioAsync(modelo));

        try
        {
            await servicioTasaCambio.ActualizarAsync(Mapear(modelo));
            TempData["MensajeExito"] = "Tasa actualizada correctamente.";
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
        var tasa = await servicioTasaCambio.ObtenerTasaPorIdAsync(id);
        return tasa is null ? NotFound() : View(tasa);
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        try
        {
            await servicioTasaCambio.EliminarAsync(id);
            TempData["MensajeExito"] = "Tasa eliminada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = $"No se pudo eliminar la tasa: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<FormularioTasaCambioViewModel> ConstruirFormularioAsync(FormularioTasaCambioViewModel modelo)
    {
        var paises = await servicioPais.ObtenerPaisesActivosAsync();
        var sucursales = await servicioSucursal.ObtenerSucursalesAsync();
        modelo.Paises = paises.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
        modelo.Sucursales = sucursales.Select(x => new OpcionSucursalViewModel { Id = x.Id, PaisId = x.PaisId, Nombre = x.Nombre }).ToList();
        return modelo;
    }

    private static TasaCambioRango Mapear(FormularioTasaCambioViewModel modelo) =>
        new()
        {
            Id = modelo.Id,
            PaisId = modelo.PaisId!.Value,
            SucursalId = modelo.SucursalId!.Value,
            MontoDesdeUsd = modelo.MontoDesdeUsd!.Value,
            MontoHastaUsd = modelo.MontoHastaUsd,
            TasaCambio = modelo.TasaCambio!.Value,
            FechaTasa = modelo.FechaTasa.Date,
            EstaActivo = modelo.EstaActivo
        };
}
