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
    IServicioTasaCambio servicioTasaCambio,
    IServicioAnaliticaConsultas servicioAnaliticaConsultas,
    ILogger<HomeController> logger) : Controller
{
    private const string NombreCookieSesionAnonima = "ElectronicaVallarta.SesionAnonima";

    public async Task<IActionResult> Index()
    {
        var paisesActivos = await servicioPais.ObtenerPaisesActivosAsync();
        var sucursales = await servicioSucursal.ObtenerSucursalesActivasAsync();
        var tasas = await servicioTasaCambio.ObtenerTasasAsync(DateTime.Today);
        var paisMexico = paisesActivos.FirstOrDefault(x => x.Nombre.Equals("Mexico", StringComparison.OrdinalIgnoreCase));
        var paisPredeterminado = paisMexico ?? paisesActivos.FirstOrDefault();
        var sucursalPredeterminada = sucursales
            .Where(x => x.PaisId == paisPredeterminado?.Id)
            .OrderBy(x => x.Nombre)
            .FirstOrDefault();

        var modelo = new CalculadoraViewModel
        {
            Solicitud = new SolicitudCalculoDto
            {
                PaisId = paisPredeterminado?.Id,
                SucursalId = sucursalPredeterminada?.Id
            },
            Paises = paisesActivos
                .Select(x => new SelectListItem(x.Nombre, x.Id.ToString(), x.Id == paisPredeterminado?.Id))
                .ToList(),
            Sucursales = sucursales
                .Select(x => new OpcionSucursalViewModel { Id = x.Id, PaisId = x.PaisId, Nombre = x.Nombre })
                .ToList(),
            TasasActivas = tasas
                .Where(x => x.EstaActivo
                            && x.Pais is not null
                            && x.Sucursal is not null)
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
                .ToList()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calcular(SolicitudCalculoDto solicitud)
    {
        var cronometro = Stopwatch.StartNew();
        ResultadoCalculoDto resultado;

        if (!ModelState.IsValid)
        {
            var mensaje = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).FirstOrDefault()
                          ?? "Verifica los datos ingresados.";
            resultado = new ResultadoCalculoDto { EsExitoso = false, Mensaje = mensaje };
        }
        else
        {
            resultado = await servicioCalculadora.CalcularAsync(solicitud);
        }

        cronometro.Stop();

        try
        {
            // La analitica no debe interrumpir la respuesta principal del simulador.
            await servicioAnaliticaConsultas.RegistrarConsultaAsync(new RegistroConsultaAnaliticaEntradaDto
            {
                FechaConsultaUtc = DateTime.UtcNow,
                PaisId = solicitud.PaisId,
                SucursalId = solicitud.SucursalId,
                TasaCambioRangoId = resultado.TasaCambioRangoId,
                RangoMontoDesdeUsd = resultado.RangoMontoDesdeUsd,
                RangoMontoHastaUsd = resultado.RangoMontoHastaUsd,
                DescripcionRango = resultado.DescripcionRangoAplicado ?? ObtenerDescripcionRangoInferido(solicitud.MontoUsd),
                MontoConsultadoUsd = solicitud.MontoUsd,
                ResultadoObtenido = resultado.EsExitoso ? resultado.MontoRecibe : null,
                TasaCambioAplicada = resultado.EsExitoso ? resultado.TasaCambioAplicada : null,
                EsExitosa = resultado.EsExitoso,
                MensajeError = resultado.EsExitoso ? null : resultado.Mensaje,
                NombrePais = string.IsNullOrWhiteSpace(resultado.NombrePais) ? null : resultado.NombrePais,
                NombreSucursal = string.IsNullOrWhiteSpace(resultado.NombreSucursal) ? null : resultado.NombreSucursal,
                IpCliente = ObtenerIpCliente(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                IdiomaNavegador = Request.Headers.AcceptLanguage.ToString(),
                RutaOrigen = Request.Path.Value ?? "/",
                Referer = Request.Headers.Referer.ToString(),
                TiempoRespuestaMs = cronometro.ElapsedMilliseconds,
                IdentificadorSesionAnonima = ObtenerSesionAnonima(),
                MetodoHttp = Request.Method
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No fue posible registrar la analitica de la consulta publica.");
        }

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

    private string ObtenerSesionAnonima()
    {
        if (Request.Cookies.TryGetValue(NombreCookieSesionAnonima, out var identificadorExistente) &&
            !string.IsNullOrWhiteSpace(identificadorExistente))
        {
            return identificadorExistente;
        }

        var identificadorNuevo = Guid.NewGuid().ToString("N");
        Response.Cookies.Append(NombreCookieSesionAnonima, identificadorNuevo, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

        return identificadorNuevo;
    }

    private string? ObtenerIpCliente()
    {
        var ipEncabezado = Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(ipEncabezado))
        {
            return ipEncabezado.Split(',').FirstOrDefault()?.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static string? ObtenerDescripcionRangoInferido(decimal? montoConsultado)
    {
        if (!montoConsultado.HasValue || montoConsultado <= 0)
        {
            return null;
        }

        return montoConsultado < 1000
            ? "0.01 - 999.99 USD"
            : "1000.00 - En adelante USD";
    }
}
