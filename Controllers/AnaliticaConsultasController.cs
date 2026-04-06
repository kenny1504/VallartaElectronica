using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicaVallarta.Controllers;

[Authorize(Roles = "Administrador")]
[Route("interno/telemetria-consultas")]
public class AnaliticaConsultasController(IServicioAnaliticaConsultas servicioAnaliticaConsultas) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? fechaDesde,
        string? fechaHasta,
        int? paisId,
        string? region,
        int? sucursalId,
        string? rango,
        bool? esExitosa,
        string? textoLibre,
        int pagina = 1,
        int tamanoPagina = 20)
    {
        var filtro = CrearFiltro(fechaDesde, fechaHasta, paisId, region, sucursalId, rango, esExitosa, textoLibre, pagina, tamanoPagina);
        var modelo = await servicioAnaliticaConsultas.ObtenerPanelAsync(filtro);
        return View(modelo);
    }

    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar(
        string? fechaDesde,
        string? fechaHasta,
        int? paisId,
        string? region,
        int? sucursalId,
        string? rango,
        bool? esExitosa,
        string? textoLibre)
    {
        var filtro = CrearFiltro(fechaDesde, fechaHasta, paisId, region, sucursalId, rango, esExitosa, textoLibre, 1, 10000);
        var archivo = await servicioAnaliticaConsultas.ExportarAsync(filtro);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    private static FiltroConsultaAnaliticaDto CrearFiltro(
        string? fechaDesde,
        string? fechaHasta,
        int? paisId,
        string? region,
        int? sucursalId,
        string? rango,
        bool? esExitosa,
        string? textoLibre,
        int pagina,
        int tamanoPagina)
    {
        return new FiltroConsultaAnaliticaDto
        {
            FechaDesde = ConvertirFecha(fechaDesde),
            FechaHasta = ConvertirFecha(fechaHasta),
            PaisId = paisId,
            Region = string.IsNullOrWhiteSpace(region) ? null : region.Trim(),
            SucursalId = sucursalId,
            Rango = string.IsNullOrWhiteSpace(rango) ? null : rango.Trim(),
            EsExitosa = esExitosa,
            TextoLibre = string.IsNullOrWhiteSpace(textoLibre) ? null : textoLibre.Trim(),
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }

    private static DateTime? ConvertirFecha(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        return DateTime.TryParseExact(valor, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var fecha)
            ? fecha
            : null;
    }
}
