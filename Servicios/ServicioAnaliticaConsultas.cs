using ClosedXML.Excel;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos.Dto;
using ElectronicaVallarta.ViewModels;

namespace ElectronicaVallarta.Servicios;

public class ServicioAnaliticaConsultas(IRepositorioConsultaAnalitica repositorioConsultaAnalitica) : IServicioAnaliticaConsultas
{
    public async Task RegistrarConsultaAsync(RegistroConsultaAnaliticaEntradaDto registroConsulta)
    {
        var entidad = new RegistroConsultaAnalitica
        {
            FechaConsultaUtc = registroConsulta.FechaConsultaUtc,
            PaisId = registroConsulta.PaisId,
            NombrePais = registroConsulta.NombrePais,
            RegionId = registroConsulta.RegionId,
            NombreRegion = registroConsulta.NombreRegion,
            SucursalId = registroConsulta.SucursalId,
            NombreSucursal = registroConsulta.NombreSucursal,
            TasaCambioRangoId = registroConsulta.TasaCambioRangoId,
            RangoMontoDesdeUsd = registroConsulta.RangoMontoDesdeUsd,
            RangoMontoHastaUsd = registroConsulta.RangoMontoHastaUsd,
            DescripcionRango = registroConsulta.DescripcionRango,
            MontoConsultadoUsd = registroConsulta.MontoConsultadoUsd,
            ResultadoObtenido = registroConsulta.ResultadoObtenido,
            TasaCambioAplicada = registroConsulta.TasaCambioAplicada,
            EsExitosa = registroConsulta.EsExitosa,
            MensajeError = registroConsulta.MensajeError,
            IpCliente = registroConsulta.IpCliente,
            UserAgent = registroConsulta.UserAgent,
            IdiomaNavegador = registroConsulta.IdiomaNavegador,
            RutaOrigen = registroConsulta.RutaOrigen,
            Referer = registroConsulta.Referer,
            TiempoRespuestaMs = registroConsulta.TiempoRespuestaMs,
            IdentificadorSesionAnonima = registroConsulta.IdentificadorSesionAnonima,
            MetodoHttp = registroConsulta.MetodoHttp,
            EstaActivo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await repositorioConsultaAnalitica.AgregarAsync(entidad);
    }

    public async Task<PanelAnaliticaConsultasViewModel> ObtenerPanelAsync(FiltroConsultaAnaliticaDto filtro)
    {
        NormalizarFiltro(filtro);

        return new PanelAnaliticaConsultasViewModel
        {
            Filtro = filtro,
            Resumen = await repositorioConsultaAnalitica.ObtenerResumenAsync(filtro),
            Listado = await repositorioConsultaAnalitica.ObtenerListadoPaginadoAsync(filtro),
            OpcionesFiltro = await repositorioConsultaAnalitica.ObtenerOpcionesFiltroAsync()
        };
    }

    public async Task<ArchivoExportacionDto> ExportarAsync(FiltroConsultaAnaliticaDto filtro)
    {
        NormalizarFiltro(filtro);
        var registros = await repositorioConsultaAnalitica.ObtenerExportacionAsync(filtro);

        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Consultas");

        var encabezados = new[]
        {
            "Fecha UTC",
            "Pais",
            "Region",
            "Sucursal",
            "Rango",
            "Monto consultado USD",
            "Resultado",
            "Tasa aplicada",
            "Tiempo respuesta ms",
            "Estado",
            "IP",
            "Sesion anonima",
            "Error"
        };

        for (var i = 0; i < encabezados.Length; i++)
        {
            hoja.Cell(1, i + 1).Value = encabezados[i];
        }

        var rangoEncabezados = hoja.Range(1, 1, 1, encabezados.Length);
        rangoEncabezados.Style.Font.Bold = true;
        rangoEncabezados.Style.Fill.BackgroundColor = XLColor.FromHtml("#DFF2FF");

        var fila = 2;
        foreach (var registro in registros)
        {
            hoja.Cell(fila, 1).Value = registro.FechaConsultaUtc;
            hoja.Cell(fila, 1).Style.DateFormat.Format = "MM/dd/yyyy HH:mm:ss";
            hoja.Cell(fila, 2).Value = registro.NombrePais ?? string.Empty;
            hoja.Cell(fila, 3).Value = registro.NombreRegion ?? string.Empty;
            hoja.Cell(fila, 4).Value = registro.NombreSucursal ?? string.Empty;
            hoja.Cell(fila, 5).Value = registro.DescripcionRango ?? string.Empty;
            hoja.Cell(fila, 6).Value = registro.MontoConsultadoUsd;
            hoja.Cell(fila, 7).Value = registro.ResultadoObtenido;
            hoja.Cell(fila, 8).Value = registro.TasaCambioAplicada;
            hoja.Cell(fila, 9).Value = registro.TiempoRespuestaMs;
            hoja.Cell(fila, 10).Value = registro.EsExitosa ? "Exitosa" : "Fallida";
            hoja.Cell(fila, 11).Value = registro.IpCliente ?? string.Empty;
            hoja.Cell(fila, 12).Value = registro.IdentificadorSesionAnonima ?? string.Empty;
            hoja.Cell(fila, 13).Value = registro.MensajeError ?? string.Empty;
            fila++;
        }

        hoja.Columns().AdjustToContents();

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);

        return new ArchivoExportacionDto
        {
            NombreArchivo = $"analitica-consultas-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx",
            TipoContenido = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Contenido = memoria.ToArray()
        };
    }

    private static void NormalizarFiltro(FiltroConsultaAnaliticaDto filtro)
    {
        filtro.Pagina = filtro.Pagina <= 0 ? 1 : filtro.Pagina;
        filtro.TamanoPagina = filtro.TamanoPagina <= 0 ? 20 : Math.Min(filtro.TamanoPagina, 200);

        if (filtro.FechaDesde.HasValue)
        {
            filtro.FechaDesde = filtro.FechaDesde.Value.Date;
        }

        if (filtro.FechaHasta.HasValue)
        {
            filtro.FechaHasta = filtro.FechaHasta.Value.Date;
        }
    }
}
