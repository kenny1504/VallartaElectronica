using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos.Dto;
using ElectronicaVallarta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace ElectronicaVallarta.Controllers;

[Authorize(Roles = "Administrador")]
public class AdministracionTasasCambioController(
    IServicioTasaCambio servicioTasaCambio,
    IServicioPais servicioPais,
    IServicioSucursal servicioSucursal,
    IServicioActualizadorPublicidadSvg servicioActualizadorPublicidadSvg) : Controller
{
    public async Task<IActionResult> Index(string? fechaFiltro, int? paisIdFiltro, bool mostrarTodos = false)
    {
        var fechaAplicada = mostrarTodos ? (DateTime?)null : ObtenerFechaFiltro(fechaFiltro);
        return View(await ConstruirListadoAsync(fechaAplicada, paisIdFiltro, mostrarTodos));
    }

    public async Task<IActionResult> Reporte(string? fechaFiltro, int? paisIdFiltro, bool mostrarTodos = false)
    {
        var fechaAplicada = mostrarTodos ? (DateTime?)null : ObtenerFechaFiltro(fechaFiltro);
        return View(await ConstruirListadoAsync(fechaAplicada, paisIdFiltro, mostrarTodos));
    }

    public async Task<IActionResult> EdicionMasiva(string? fechaFiltro, int? paisId)
    {
        var fechaTasa = ObtenerFechaFiltroEdicionMasiva(fechaFiltro);
        return View(await ConstruirEdicionMasivaAsync(fechaTasa, paisId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EdicionMasiva(EdicionMasivaTasasCambioViewModel modelo)
    {
        if (!ModelState.IsValid || !modelo.PaisId.HasValue)
        {
            return View(await ConstruirEdicionMasivaAsync(modelo.FechaTasa, modelo.PaisId, modelo.Tasas));
        }

        try
        {
            var tasasActualizadas = await servicioTasaCambio.ActualizarTasasEnLoteAsync(
                modelo.FechaTasa,
                modelo.PaisId.Value,
                modelo.Tasas.Select(x => new ActualizacionTasaCambioMasivaDto
                {
                    Id = x.Id,
                    TasaCambio = x.TasaCambio!.Value
                }).ToList());

            TempData["MensajeExito"] = $"{tasasActualizadas} tasa(s) actualizadas para {modelo.FechaTasa:MM/dd/yyyy}.";
            return RedirectToAction(nameof(Index), new
            {
                fechaFiltro = modelo.FechaTasa.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                paisIdFiltro = modelo.PaisId
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await ConstruirEdicionMasivaAsync(modelo.FechaTasa, modelo.PaisId, modelo.Tasas));
        }
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copiar(CopiarTasasCambioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            TempData["MensajeError"] = "Selecciona la fecha destino para copiar las tasas.";
            return RedireccionarAListado(modelo.FechaOrigen, modelo.PaisIdFiltro);
        }

        try
        {
            var tasasCopiadas = await servicioTasaCambio.CopiarAsync(modelo.FechaOrigen, modelo.FechaDestino, modelo.CopiarTodas, modelo.TasasSeleccionadas, modelo.PaisIdFiltro);
            TempData["MensajeExito"] = $"{tasasCopiadas} tasa(s) copiadas al {modelo.FechaDestino:MM/dd/yyyy}.";
            return RedirectToAction(nameof(Index), new { fechaFiltro = modelo.FechaDestino.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), modelo.PaisIdFiltro });
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = $"No se pudieron copiar las tasas: {ex.Message}";
            return RedireccionarAListado(modelo.FechaOrigen, modelo.PaisIdFiltro);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarPublicidadSvg(string fechaTasa)
    {
        if (!DateTime.TryParseExact(fechaTasa, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaParseada))
        {
            return Json(new { success = false, message = "La fecha seleccionada no es valida." });
        }

        var resultado = await servicioActualizadorPublicidadSvg.ActualizarAsync(fechaParseada.Date);
        return Json(new { success = resultado.Success, message = resultado.Message });
    }

    private async Task<FormularioTasaCambioViewModel> ConstruirFormularioAsync(FormularioTasaCambioViewModel modelo)
    {
        var paises = await servicioPais.ObtenerPaisesActivosAsync();
        var sucursales = await servicioSucursal.ObtenerSucursalesAsync();
        modelo.Paises = paises.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
        modelo.Sucursales = sucursales.Select(x => new OpcionSucursalViewModel { Id = x.Id, PaisId = x.PaisId, Nombre = x.Nombre }).ToList();
        return modelo;
    }

    private async Task<ListadoTasasCambioViewModel> ConstruirListadoAsync(DateTime? fechaFiltro, int? paisIdFiltro, bool mostrarTodos)
    {
        var paises = await servicioPais.ObtenerPaisesActivosAsync();
        return new ListadoTasasCambioViewModel
        {
            FechaFiltro = fechaFiltro,
            PaisIdFiltro = paisIdFiltro,
            MostrarTodos = mostrarTodos,
            Paises = paises.Select(x => new SelectListItem(x.Nombre, x.Id.ToString(), x.Id == paisIdFiltro)).ToList(),
            Tasas = await servicioTasaCambio.ObtenerListadoTasasAsync(fechaFiltro, paisIdFiltro)
        };
    }

    private async Task<EdicionMasivaTasasCambioViewModel> ConstruirEdicionMasivaAsync(
        DateTime fechaTasa,
        int? paisId,
        IReadOnlyCollection<TasaCambioEdicionMasivaItemViewModel>? tasasEditadas = null)
    {
        var paises = await servicioPais.ObtenerPaisesActivosAsync();
        var valoresEditados = (tasasEditadas ?? [])
            .Where(x => x.Id > 0)
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.First().TasaCambio);
        var tasas = paisId.HasValue
            ? (await servicioTasaCambio.ObtenerTasasAsync(fechaTasa.Date))
                .Where(x => x.PaisId == paisId.Value)
                .OrderBy(x => x.Sucursal!.Nombre)
                .ThenBy(x => x.MontoDesdeUsd)
                .ToList()
            : [];

        return new EdicionMasivaTasasCambioViewModel
        {
            FechaTasa = fechaTasa.Date,
            PaisId = paisId,
            NombrePais = paises.FirstOrDefault(x => x.Id == paisId)?.Nombre,
            Paises = paises.Select(x => new SelectListItem(x.Nombre, x.Id.ToString(), x.Id == paisId)).ToList(),
            Tasas = tasas.Select(x => new TasaCambioEdicionMasivaItemViewModel
            {
                Id = x.Id,
                TasaCambio = valoresEditados.GetValueOrDefault(x.Id, x.TasaCambio),
                NombreSucursal = x.Sucursal?.Nombre ?? string.Empty,
                MontoDesdeUsd = x.MontoDesdeUsd,
                MontoHastaUsd = x.MontoHastaUsd,
                EstaActivo = x.EstaActivo
            }).ToList()
        };
    }

    private static DateTime ObtenerFechaFiltro(string? fechaFiltro)
    {
        if (!string.IsNullOrWhiteSpace(fechaFiltro) &&
            DateTime.TryParseExact(fechaFiltro, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaParseada))
        {
            return fechaParseada.Date;
        }

        return DateTime.Today;
    }

    private static DateTime ObtenerFechaFiltroEdicionMasiva(string? fechaFiltro)
    {
        if (!string.IsNullOrWhiteSpace(fechaFiltro) &&
            DateTime.TryParseExact(fechaFiltro, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaParseada))
        {
            return fechaParseada.Date;
        }

        return DateTime.Today;
    }

    private RedirectToActionResult RedireccionarAListado(DateTime? fechaFiltro, int? paisIdFiltro)
    {
        return RedirectToAction(nameof(Index), new { fechaFiltro = fechaFiltro?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), paisIdFiltro });
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
