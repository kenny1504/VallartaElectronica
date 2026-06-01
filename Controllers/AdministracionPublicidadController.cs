using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Dominio.Enumeraciones;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.Controllers;

[Authorize(Roles = "Administrador")]
public class AdministracionPublicidadController(IServicioPublicidad servicioPublicidad) : Controller
{
    public async Task<IActionResult> Index() => View(await servicioPublicidad.ObtenerPublicidadesAsync());

    public IActionResult Crear() => View(ConstruirFormulario(new FormularioPublicidadViewModel { EstaActivo = true, DuracionSegundos = 8 }));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(FormularioPublicidadViewModel modelo)
    {
        if (!ModelState.IsValid) return View(ConstruirFormulario(modelo));

        try
        {
            await servicioPublicidad.CrearAsync(MapearEntidad(modelo), modelo.Archivo);
            TempData["MensajeExito"] = "Publicidad creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(ConstruirFormulario(modelo));
        }
    }

    public async Task<IActionResult> Editar(int id)
    {
        var publicidad = await servicioPublicidad.ObtenerPublicidadPorIdAsync(id);
        if (publicidad is null) return NotFound();

        return View(ConstruirFormulario(new FormularioPublicidadViewModel
        {
            Id = publicidad.Id,
            Titulo = publicidad.Titulo,
            Descripcion = publicidad.Descripcion,
            TipoRecurso = publicidad.TipoRecurso,
            UrlRecursoActual = publicidad.UrlRecurso,
            DuracionSegundos = publicidad.DuracionSegundos,
            Orden = publicidad.Orden,
            EstaActivo = publicidad.EstaActivo,
            FechaInicio = publicidad.FechaInicio,
            FechaFin = publicidad.FechaFin
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(FormularioPublicidadViewModel modelo)
    {
        if (!ModelState.IsValid) return View(ConstruirFormulario(modelo));

        try
        {
            await servicioPublicidad.ActualizarAsync(MapearEntidad(modelo), modelo.Archivo);
            TempData["MensajeExito"] = "Publicidad actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(ConstruirFormulario(modelo));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        try
        {
            await servicioPublicidad.CambiarEstadoAsync(id);
            TempData["MensajeExito"] = "Estado de publicidad actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Eliminar(int id)
    {
        var publicidad = await servicioPublicidad.ObtenerPublicidadPorIdAsync(id);
        return publicidad is null ? NotFound() : View(publicidad);
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        try
        {
            await servicioPublicidad.EliminarAsync(id);
            TempData["MensajeExito"] = "Publicidad eliminada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = $"No se pudo eliminar la publicidad: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private static FormularioPublicidadViewModel ConstruirFormulario(FormularioPublicidadViewModel modelo)
    {
        modelo.TiposRecurso = Enum.GetValues<TipoRecursoPublicidad>()
            .Select(x => new SelectListItem(x.ToString(), ((int)x).ToString()))
            .ToList();
        return modelo;
    }

    private static Publicidad MapearEntidad(FormularioPublicidadViewModel modelo) => new()
    {
        Id = modelo.Id,
        Titulo = modelo.Titulo,
        Descripcion = modelo.Descripcion,
        TipoRecurso = modelo.TipoRecurso!.Value,
        DuracionSegundos = modelo.DuracionSegundos,
        Orden = modelo.Orden,
        EstaActivo = modelo.EstaActivo,
        FechaInicio = modelo.FechaInicio,
        FechaFin = modelo.FechaFin
    };
}
