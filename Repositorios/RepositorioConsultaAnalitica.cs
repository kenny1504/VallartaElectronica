using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Modelos.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ElectronicaVallarta.Repositorios;

public class RepositorioConsultaAnalitica(ContextoAplicacion contexto) : IRepositorioConsultaAnalitica
{
    public async Task AgregarAsync(RegistroConsultaAnalitica registroConsultaAnalitica)
    {
        await contexto.RegistrosConsultasAnalitica.AddAsync(registroConsultaAnalitica);
        await contexto.SaveChangesAsync();
    }

    public async Task<ResumenDashboardAnaliticaDto> ObtenerResumenAsync(FiltroConsultaAnaliticaDto filtro)
    {
        var consulta = AplicarFiltro(contexto.RegistrosConsultasAnalitica.AsNoTracking(), filtro);
        var hoyUtc = DateTime.UtcNow.Date;
        var haceSieteDiasUtc = hoyUtc.AddDays(-6);

        var totalConsultas = await consulta.CountAsync();
        var totalConsultasExitosas = await consulta.CountAsync(x => x.EsExitosa);
        var totalConsultasFallidas = totalConsultas - totalConsultasExitosas;
        var totalSesionesUnicas = await consulta
            .Where(x => x.IdentificadorSesionAnonima != null && x.IdentificadorSesionAnonima != string.Empty)
            .Select(x => x.IdentificadorSesionAnonima!)
            .Distinct()
            .CountAsync();
        var totalConsultasHoy = await consulta.CountAsync(x => x.FechaConsultaUtc >= hoyUtc && x.FechaConsultaUtc < hoyUtc.AddDays(1));
        var totalConsultasUltimosSieteDias = await consulta.CountAsync(x => x.FechaConsultaUtc >= haceSieteDiasUtc && x.FechaConsultaUtc < hoyUtc.AddDays(1));
        var totalConsultasMontoMayorMil = await consulta.CountAsync(x => x.MontoConsultadoUsd.HasValue && x.MontoConsultadoUsd >= 1000);
        var promedioTiempoRespuesta = await consulta.Select(x => (decimal?)x.TiempoRespuestaMs).AverageAsync() ?? 0m;
        var promedioMontoConsultadoUsd = await consulta.Select(x => x.MontoConsultadoUsd).AverageAsync() ?? 0m;
        var promedioResultadoObtenido = await consulta.Where(x => x.EsExitosa).Select(x => x.ResultadoObtenido).AverageAsync() ?? 0m;
        var promedioTasaAplicada = await consulta.Where(x => x.EsExitosa).Select(x => x.TasaCambioAplicada).AverageAsync() ?? 0m;

        var seriePorDia = await consulta
            .GroupBy(x => x.FechaConsultaUtc.Date)
            .Select(x => new SerieDiariaConsultaAnaliticaDto
            {
                Fecha = x.Key,
                Total = x.Count()
            })
            .OrderBy(x => x.Fecha)
            .ToListAsync();

        var topPaises = await ObtenerTopAsync(consulta, x => x.NombrePais);
        var topRegiones = await ObtenerTopAsync(consulta, x => x.NombreRegion);
        var topSucursales = await ObtenerTopAsync(consulta, x => x.NombreSucursal);
        var topRangos = await ObtenerTopAsync(consulta, x => x.DescripcionRango);
        var topErrores = await consulta
            .Where(x => !x.EsExitosa && x.MensajeError != null && x.MensajeError != string.Empty)
            .GroupBy(x => x.MensajeError!)
            .Select(x => new ElementoMetricaAnaliticaDto
            {
                Etiqueta = x.Key,
                Total = x.Count()
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Etiqueta)
            .Take(5)
            .ToListAsync();
        // El etiquetado final se arma en memoria porque EF no traduce bien string.Format sobre GroupBy.
        var topCombinacionesBase = await consulta
            .Where(x => x.NombrePais != null && x.NombreSucursal != null)
            .GroupBy(x => new { x.NombrePais, x.NombreSucursal, x.DescripcionRango })
            .Select(x => new
            {
                x.Key.NombrePais,
                x.Key.NombreSucursal,
                x.Key.DescripcionRango,
                Total = x.Count()
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.NombrePais)
            .ThenBy(x => x.NombreSucursal)
            .ThenBy(x => x.DescripcionRango)
            .Take(5)
            .ToListAsync();

        var topCombinaciones = topCombinacionesBase
            .Select(x => new ElementoMetricaAnaliticaDto
            {
                Etiqueta = $"{x.NombrePais} / {x.NombreSucursal} / {x.DescripcionRango ?? "Sin rango"}",
                Total = x.Total
            })
            .ToList();

        return new ResumenDashboardAnaliticaDto
        {
            TotalConsultas = totalConsultas,
            TotalConsultasHoy = totalConsultasHoy,
            TotalConsultasUltimosSieteDias = totalConsultasUltimosSieteDias,
            TotalSesionesUnicas = totalSesionesUnicas,
            TotalConsultasExitosas = totalConsultasExitosas,
            TotalConsultasFallidas = totalConsultasFallidas,
            TotalConsultasMontoMayorMil = totalConsultasMontoMayorMil,
            PromedioTiempoRespuestaMs = Math.Round(promedioTiempoRespuesta, 2),
            PorcentajeExito = totalConsultas == 0 ? 0 : Math.Round((decimal)totalConsultasExitosas * 100m / totalConsultas, 2),
            PromedioMontoConsultadoUsd = Math.Round(promedioMontoConsultadoUsd, 2),
            PromedioResultadoObtenido = Math.Round(promedioResultadoObtenido, 2),
            PromedioTasaAplicada = Math.Round(promedioTasaAplicada, 4),
            PaisMasConsultado = topPaises.FirstOrDefault()?.Etiqueta ?? "Sin datos",
            RegionMasConsultada = topRegiones.FirstOrDefault()?.Etiqueta ?? "Sin datos",
            SucursalMasConsultada = topSucursales.FirstOrDefault()?.Etiqueta ?? "Sin datos",
            RangoMasConsultado = topRangos.FirstOrDefault()?.Etiqueta ?? "Sin datos",
            SeriePorDia = seriePorDia,
            TopPaises = topPaises,
            TopRegiones = topRegiones,
            TopSucursales = topSucursales,
            TopRangos = topRangos,
            TopErrores = topErrores,
            TopCombinaciones = topCombinaciones
        };
    }

    public async Task<PaginacionConsultaAnaliticaDto> ObtenerListadoPaginadoAsync(FiltroConsultaAnaliticaDto filtro)
    {
        var pagina = filtro.Pagina <= 0 ? 1 : filtro.Pagina;
        var tamanoPagina = filtro.TamanoPagina <= 0 ? 20 : Math.Min(filtro.TamanoPagina, 200);
        var consulta = AplicarFiltro(contexto.RegistrosConsultasAnalitica.AsNoTracking(), filtro);
        var totalRegistros = await consulta.CountAsync();

        var registros = await consulta
            .OrderByDescending(x => x.FechaConsultaUtc)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(x => new RegistroConsultaAnaliticaListadoDto
            {
                Id = x.Id,
                FechaConsultaUtc = x.FechaConsultaUtc,
                NombrePais = x.NombrePais,
                NombreRegion = x.NombreRegion,
                NombreSucursal = x.NombreSucursal,
                DescripcionRango = x.DescripcionRango,
                MontoConsultadoUsd = x.MontoConsultadoUsd,
                ResultadoObtenido = x.ResultadoObtenido,
                TasaCambioAplicada = x.TasaCambioAplicada,
                TiempoRespuestaMs = x.TiempoRespuestaMs,
                EsExitosa = x.EsExitosa,
                IpCliente = x.IpCliente,
                IdentificadorSesionAnonima = x.IdentificadorSesionAnonima,
                MensajeError = x.MensajeError
            })
            .ToListAsync();

        return new PaginacionConsultaAnaliticaDto
        {
            TotalRegistros = totalRegistros,
            PaginaActual = pagina,
            TamanoPagina = tamanoPagina,
            Registros = registros
        };
    }

    public async Task<IReadOnlyCollection<RegistroConsultaAnaliticaListadoDto>> ObtenerExportacionAsync(FiltroConsultaAnaliticaDto filtro)
    {
        var consulta = AplicarFiltro(contexto.RegistrosConsultasAnalitica.AsNoTracking(), filtro);

        return await consulta
            .OrderByDescending(x => x.FechaConsultaUtc)
            .Select(x => new RegistroConsultaAnaliticaListadoDto
            {
                Id = x.Id,
                FechaConsultaUtc = x.FechaConsultaUtc,
                NombrePais = x.NombrePais,
                NombreRegion = x.NombreRegion,
                NombreSucursal = x.NombreSucursal,
                DescripcionRango = x.DescripcionRango,
                MontoConsultadoUsd = x.MontoConsultadoUsd,
                ResultadoObtenido = x.ResultadoObtenido,
                TasaCambioAplicada = x.TasaCambioAplicada,
                TiempoRespuestaMs = x.TiempoRespuestaMs,
                EsExitosa = x.EsExitosa,
                IpCliente = x.IpCliente,
                IdentificadorSesionAnonima = x.IdentificadorSesionAnonima,
                MensajeError = x.MensajeError
            })
            .ToListAsync();
    }

    public async Task<OpcionesFiltroAnaliticaDto> ObtenerOpcionesFiltroAsync()
    {
        var consulta = contexto.RegistrosConsultasAnalitica.AsNoTracking();

        var paises = await consulta
            .Where(x => x.PaisId.HasValue && x.NombrePais != null)
            .GroupBy(x => new { x.PaisId, x.NombrePais })
            .OrderBy(x => x.Key.NombrePais)
            .Select(x => new OpcionFiltroAnaliticaDto
            {
                Valor = x.Key.PaisId!.Value.ToString(),
                Etiqueta = x.Key.NombrePais!
            })
            .ToListAsync();

        var regiones = await consulta
            .Where(x => x.NombreRegion != null && x.NombreRegion != string.Empty)
            .Select(x => x.NombreRegion!)
            .Distinct()
            .OrderBy(x => x)
            .Select(x => new OpcionFiltroAnaliticaDto { Valor = x, Etiqueta = x })
            .ToListAsync();

        var sucursales = await consulta
            .Where(x => x.SucursalId.HasValue && x.NombreSucursal != null)
            .GroupBy(x => new { x.SucursalId, x.NombreSucursal })
            .OrderBy(x => x.Key.NombreSucursal)
            .Select(x => new OpcionFiltroAnaliticaDto
            {
                Valor = x.Key.SucursalId!.Value.ToString(),
                Etiqueta = x.Key.NombreSucursal!
            })
            .ToListAsync();

        var rangos = await consulta
            .Where(x => x.DescripcionRango != null && x.DescripcionRango != string.Empty)
            .Select(x => x.DescripcionRango!)
            .Distinct()
            .OrderBy(x => x)
            .Select(x => new OpcionFiltroAnaliticaDto { Valor = x, Etiqueta = x })
            .ToListAsync();

        return new OpcionesFiltroAnaliticaDto
        {
            Paises = paises,
            Regiones = regiones,
            Sucursales = sucursales,
            Rangos = rangos
        };
    }

    private static IQueryable<RegistroConsultaAnalitica> AplicarFiltro(IQueryable<RegistroConsultaAnalitica> consulta, FiltroConsultaAnaliticaDto filtro)
    {
        // Mantener el filtro como IQueryable evita llevar agregaciones y paginacion a memoria.
        if (filtro.FechaDesde.HasValue)
        {
            var fechaDesde = filtro.FechaDesde.Value.Date;
            consulta = consulta.Where(x => x.FechaConsultaUtc >= fechaDesde);
        }

        if (filtro.FechaHasta.HasValue)
        {
            var fechaHasta = filtro.FechaHasta.Value.Date.AddDays(1);
            consulta = consulta.Where(x => x.FechaConsultaUtc < fechaHasta);
        }

        if (filtro.PaisId.HasValue)
        {
            consulta = consulta.Where(x => x.PaisId == filtro.PaisId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Region))
        {
            consulta = consulta.Where(x => x.NombreRegion == filtro.Region);
        }

        if (filtro.SucursalId.HasValue)
        {
            consulta = consulta.Where(x => x.SucursalId == filtro.SucursalId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Rango))
        {
            consulta = consulta.Where(x => x.DescripcionRango == filtro.Rango);
        }

        if (filtro.EsExitosa.HasValue)
        {
            consulta = consulta.Where(x => x.EsExitosa == filtro.EsExitosa.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.TextoLibre))
        {
            var texto = filtro.TextoLibre.Trim();
            consulta = consulta.Where(x =>
                (x.NombrePais != null && x.NombrePais.Contains(texto)) ||
                (x.NombreRegion != null && x.NombreRegion.Contains(texto)) ||
                (x.NombreSucursal != null && x.NombreSucursal.Contains(texto)) ||
                (x.DescripcionRango != null && x.DescripcionRango.Contains(texto)) ||
                (x.IpCliente != null && x.IpCliente.Contains(texto)) ||
                (x.MensajeError != null && x.MensajeError.Contains(texto)) ||
                (x.IdentificadorSesionAnonima != null && x.IdentificadorSesionAnonima.Contains(texto)));
        }

        return consulta;
    }

    private static Task<List<ElementoMetricaAnaliticaDto>> ObtenerTopAsync(
        IQueryable<RegistroConsultaAnalitica> consulta,
        Expression<Func<RegistroConsultaAnalitica, string?>> selector)
    {
        return consulta
            .Select(selector)
            .Where(x => x != null && x != string.Empty)
            .GroupBy(x => x!)
            .Select(x => new ElementoMetricaAnaliticaDto
            {
                Etiqueta = x.Key,
                Total = x.Count()
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Etiqueta)
            .Take(5)
            .ToListAsync();
    }
}
