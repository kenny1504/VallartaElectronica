using System.Globalization;
using System.Xml.Linq;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;

namespace ElectronicaVallarta.Servicios;

public class ServicioActualizadorPublicidadSvg(
    IRepositorioTasaCambio repositorioTasaCambio,
    IWebHostEnvironment entorno,
    ILogger<ServicioActualizadorPublicidadSvg> logger) : IServicioActualizadorPublicidadSvg
{
    private const string RutaRelativaSvg = "uploads/publicidad/tasas.svg";

    private static readonly IReadOnlyCollection<ConfiguracionTasaSvg> Configuraciones =
    [
        new("rate_menos1000_elektra", 1, 2, 1, 1000),
        new("rate_mas1000_elektra", 1, 2, 1001, 2999),
        new("rate_menos1000_bancoppel", 1, 1, 1, 1000),
        new("rate_mas1000_bancoppel", 1, 1, 1001, 2999),
        new("rate_menos1000_deposito", 1, 3, 1, 1000),
        new("rate_mas1000_deposito", 1, 3, 1001, 2999)
    ];

    public async Task<ResultadoActualizacionPublicidadSvg> ActualizarAsync()
    {
        var rutaSvg = Path.Combine(entorno.WebRootPath, RutaRelativaSvg);

        try
        {
            if (!File.Exists(rutaSvg))
            {
                logger.LogError("No se encontro el archivo SVG de publicidad en {RutaSvg}.", rutaSvg);
                return new ResultadoActualizacionPublicidadSvg(false, "No se encontro el archivo SVG de publicidad.");
            }

            var documento = XDocument.Load(rutaSvg, LoadOptions.PreserveWhitespace);
            var tasasActualizadas = 0;

            foreach (var configuracion in Configuraciones)
            {
                var tasa = await repositorioTasaCambio.ObtenerTasaVigentePorRangoAsync(
                    configuracion.PaisId,
                    configuracion.SucursalId,
                    configuracion.MontoDesdeUsd,
                    configuracion.MontoHastaUsd);

                if (tasa is null)
                {
                    logger.LogWarning(
                        "No se encontro tasa vigente para el nodo SVG {NodoId}. PaisId: {PaisId}, SucursalId: {SucursalId}, MontoDesdeUsd: {MontoDesdeUsd}, MontoHastaUsd: {MontoHastaUsd}.",
                        configuracion.NodoId,
                        configuracion.PaisId,
                        configuracion.SucursalId,
                        configuracion.MontoDesdeUsd,
                        configuracion.MontoHastaUsd);
                    continue;
                }

                var nodo = documento.Descendants().FirstOrDefault(x => string.Equals((string?)x.Attribute("id"), configuracion.NodoId, StringComparison.Ordinal));
                if (nodo is null)
                {
                    logger.LogWarning("No se encontro el nodo SVG con id {NodoId}.", configuracion.NodoId);
                    continue;
                }

                nodo.Value = tasa.TasaCambio.ToString("0.00", CultureInfo.InvariantCulture);
                tasasActualizadas++;
            }

            if (tasasActualizadas == 0)
            {
                return new ResultadoActualizacionPublicidadSvg(false, "No se encontro ninguna tasa vigente para actualizar la publicidad.");
            }

            documento.Save(rutaSvg, SaveOptions.DisableFormatting);
            logger.LogInformation("Publicidad SVG actualizada correctamente. Tasas actualizadas: {TasasActualizadas}.", tasasActualizadas);

            return new ResultadoActualizacionPublicidadSvg(true, "Publicidad actualizada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo actualizar la publicidad SVG en {RutaSvg}.", rutaSvg);
            return new ResultadoActualizacionPublicidadSvg(false, "No se pudo actualizar la publicidad SVG. Intentalo nuevamente.");
        }
    }

    private sealed record ConfiguracionTasaSvg(string NodoId, int PaisId, int SucursalId, decimal MontoDesdeUsd, decimal? MontoHastaUsd);
}
