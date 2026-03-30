using System.Diagnostics;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos;
using ElectronicaVallarta.Modelos.Dto;
using ElectronicaVallarta.Models;
using ElectronicaVallarta.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.Controllers;

public class HomeController(
    IServicioPais servicioPais,
    IServicioSucursal servicioSucursal,
    IServicioCalculadora servicioCalculadora,
    IServicioTasaCambio servicioTasaCambio) : Controller
{
    public async Task<IActionResult> Index()
    {
        var paisesActivos = await servicioPais.ObtenerPaisesActivosAsync();
        var sucursales = await servicioSucursal.ObtenerSucursalesAsync();
        var tasas = await servicioTasaCambio.ObtenerTasasAsync();
        var paisMexico = paisesActivos.FirstOrDefault(x => x.Nombre.Equals("Mexico", StringComparison.OrdinalIgnoreCase));
        var paisesVisibles = paisMexico is not null ? [paisMexico] : paisesActivos;
        var paisPredeterminado = paisMexico ?? paisesActivos.FirstOrDefault();
        var sucursalPredeterminada = sucursales
            .Where(x => x.EstaActivo && x.PaisId == paisPredeterminado?.Id)
            .OrderBy(x => x.Nombre)
            .FirstOrDefault();

        var modelo = new CalculadoraViewModel
        {
            Solicitud = new SolicitudCalculoDto
            {
                PaisId = paisPredeterminado?.Id,
                SucursalId = sucursalPredeterminada?.Id
            },
            Paises = paisesVisibles
                .Select(x => new SelectListItem(x.Nombre, x.Id.ToString(), x.Id == paisPredeterminado?.Id))
                .ToList(),
            Sucursales = sucursales
                .Where(x => x.EstaActivo
                            && x.Pais is not null
                            && x.Pais.EstaActivo
                            && (paisPredeterminado is null || x.PaisId == paisPredeterminado.Id))
                .Select(x => new OpcionSucursalViewModel { Id = x.Id, PaisId = x.PaisId, Nombre = x.Nombre })
                .OrderBy(x => x.Nombre)
                .ToList(),
            TasasActivas = tasas
                .Where(x => x.EstaActivo
                            && x.Pais is not null
                            && x.Sucursal is not null
                            && x.FechaTasa.Date == DateTime.Today
                            && (paisPredeterminado is null || x.PaisId == paisPredeterminado.Id))
                .Select(x => new TasaActivaViewModel
                {
                    PaisId = x.PaisId,
                    SucursalId = x.SucursalId,
                    NombreSucursal = x.Sucursal!.Nombre,
                    NombrePais = x.Pais!.Nombre,
                    MontoDesdeUsd = x.MontoDesdeUsd,
                    MontoHastaUsd = x.MontoHastaUsd,
                    TasaCambio = x.TasaCambio,
                    CodigoMoneda = x.Pais!.CodigoMoneda
                })
                .OrderBy(x => x.MontoDesdeUsd)
                .ThenBy(x => x.NombreSucursal)
                .ToList()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calcular(SolicitudCalculoDto solicitud)
    {
        if (!ModelState.IsValid)
        {
            var mensaje = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).FirstOrDefault()
                          ?? "Verifica los datos ingresados.";
            return PartialView("_ResultadoCalculo", new ResultadoCalculoDto { EsExitoso = false, Mensaje = mensaje });
        }

        var resultado = await servicioCalculadora.CalcularAsync(solicitud);
        return PartialView("_ResultadoCalculo", resultado);
    }

    [HttpGet("/error/{codigoEstado:int?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? codigoEstado = null)
    {
        if (codigoEstado == StatusCodes.Status404NotFound)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NoEncontrado");
        }

        if (codigoEstado.HasValue)
        {
            Response.StatusCode = codigoEstado.Value;
        }

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
