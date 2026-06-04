using System.Globalization;
using System.Text;
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
        new("rate_menos1000_elektra", "Mexico", ["Elektra", "Banco Azteca"], 1, 1000),
        new("rate_mas1000_elektra", "Mexico", ["Elektra", "Banco Azteca"], 1001, 2999),
        new("rate_menos1000_bancoppel", "Mexico", ["BanCoppel"], 1, 1000),
        new("rate_mas1000_bancoppel", "Mexico", ["BanCoppel"], 1001, 2999),
        new("rate_menos1000_deposito", "Mexico", ["Deposito a cuenta", "Depósito a cuenta"], 1, 1000),
        new("rate_mas1000_deposito", "Mexico", ["Deposito a cuenta", "Depósito a cuenta"], 1001, 2999)
    ];

    public async Task<ResultadoActualizacionPublicidadSvg> ActualizarAsync(DateTime fechaTasa)
    {
        var rutaSvg = Path.Combine(entorno.WebRootPath, RutaRelativaSvg);
        var fecha = fechaTasa.Date;

        try
        {
            logger.LogInformation(
                "Iniciando actualizacion de publicidad SVG. WebRootPath: {WebRootPath}. RutaSvg: {RutaSvg}. DirectorioExiste: {DirectorioExiste}. ArchivoExiste: {ArchivoExiste}. AtributosArchivo: {AtributosArchivo}. TamanoBytes: {TamanoBytes}. UsuarioProceso: {UsuarioProceso}.",
                entorno.WebRootPath,
                rutaSvg,
                Directory.Exists(Path.GetDirectoryName(rutaSvg)),
                File.Exists(rutaSvg),
                File.Exists(rutaSvg) ? File.GetAttributes(rutaSvg).ToString() : "No existe",
                File.Exists(rutaSvg) ? new FileInfo(rutaSvg).Length : 0,
                Environment.UserName);

            if (!File.Exists(rutaSvg))
            {
                logger.LogError("No se encontro el archivo SVG de publicidad en {RutaSvg}.", rutaSvg);
                return new ResultadoActualizacionPublicidadSvg(false, "No se encontro el archivo SVG de publicidad.");
            }

            var documento = XDocument.Load(rutaSvg, LoadOptions.PreserveWhitespace);
            var tasasActualizadas = 0;
            var valoresEsperados = new Dictionary<string, string>();
            var tasasFecha = await repositorioTasaCambio.ObtenerTodosAsync(fecha);

            foreach (var configuracion in Configuraciones)
            {
                var tasa = ObtenerTasaConfigurada(tasasFecha, configuracion);

                if (tasa is null)
                {
                    logger.LogWarning(
                        "No se encontro tasa vigente para el nodo SVG {NodoId}. FechaTasa: {FechaTasa}, Pais: {Pais}, SucursalesEsperadas: {SucursalesEsperadas}, MontoDesdeUsd: {MontoDesdeUsd}, MontoHastaUsd: {MontoHastaUsd}. TasasDisponibles: {TasasDisponibles}.",
                        configuracion.NodoId,
                        fecha,
                        configuracion.PaisNombre,
                        string.Join(", ", configuracion.SucursalesEsperadas),
                        configuracion.MontoDesdeUsd,
                        configuracion.MontoHastaUsd,
                        ConstruirResumenTasas(tasasFecha));
                    continue;
                }

                var nodo = documento.Descendants().FirstOrDefault(x => string.Equals((string?)x.Attribute("id"), configuracion.NodoId, StringComparison.Ordinal));
                if (nodo is null)
                {
                    logger.LogWarning("No se encontro el nodo SVG con id {NodoId}.", configuracion.NodoId);
                    continue;
                }

                var valorTasa = tasa.TasaCambio.ToString("0.00", CultureInfo.InvariantCulture);
                nodo.Value = valorTasa;
                valoresEsperados[configuracion.NodoId] = valorTasa;
                tasasActualizadas++;

                logger.LogInformation(
                    "Nodo SVG {NodoId} actualizado con TasaCambioRangoId {TasaCambioRangoId}. PaisId: {PaisId}. Pais: {Pais}. SucursalId: {SucursalId}. Sucursal: {Sucursal}. Rango: {MontoDesdeUsd}-{MontoHastaUsd}. Valor: {Valor}.",
                    configuracion.NodoId,
                    tasa.Id,
                    tasa.PaisId,
                    tasa.Pais?.Nombre,
                    tasa.SucursalId,
                    tasa.Sucursal?.Nombre,
                    tasa.MontoDesdeUsd,
                    tasa.MontoHastaUsd,
                    valorTasa);
            }

            if (tasasActualizadas == 0)
            {
                return new ResultadoActualizacionPublicidadSvg(false, $"No se encontro ninguna tasa vigente para actualizar la publicidad del {fecha:MM/dd/yyyy}.");
            }

            GuardarSvg(rutaSvg, documento);
            var verificacion = VerificarSvgGuardado(rutaSvg, valoresEsperados);
            if (!verificacion.EsValida)
            {
                logger.LogError(
                    "El SVG fue procesado pero no se pudo confirmar la escritura en disco. RutaSvg: {RutaSvg}. Detalle: {Detalle}.",
                    rutaSvg,
                    verificacion.Detalle);
                return new ResultadoActualizacionPublicidadSvg(false, "El servidor no pudo confirmar que el archivo SVG quedara actualizado. Revisa permisos de escritura sobre wwwroot/uploads/publicidad/tasas.svg.");
            }

            var fechaActualizacion = File.GetLastWriteTimeUtc(rutaSvg);
            logger.LogInformation(
                "Publicidad SVG actualizada y verificada correctamente. RutaSvg: {RutaSvg}. FechaTasa: {FechaTasa}. Tasas actualizadas: {TasasActualizadas}. UltimaEscrituraUtc: {UltimaEscrituraUtc}.",
                rutaSvg,
                fecha,
                tasasActualizadas,
                fechaActualizacion);

            return new ResultadoActualizacionPublicidadSvg(true, "Publicidad actualizada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo actualizar la publicidad SVG en {RutaSvg}.", rutaSvg);
            return new ResultadoActualizacionPublicidadSvg(false, "No se pudo actualizar la publicidad SVG. Intentalo nuevamente.");
        }
    }

    private static ElectronicaVallarta.Dominio.Entidades.TasaCambioRango? ObtenerTasaConfigurada(
        IEnumerable<ElectronicaVallarta.Dominio.Entidades.TasaCambioRango> tasas,
        ConfiguracionTasaSvg configuracion)
    {
        var paisEsperado = NormalizarTexto(configuracion.PaisNombre);
        var sucursalesEsperadas = configuracion.SucursalesEsperadas.Select(NormalizarTexto).ToList();

        return tasas
            .Where(x => x.EstaActivo &&
                        string.Equals(NormalizarTexto(x.Pais?.Nombre), paisEsperado, StringComparison.Ordinal) &&
                        x.MontoDesdeUsd == configuracion.MontoDesdeUsd &&
                        x.MontoHastaUsd == configuracion.MontoHastaUsd &&
                        SucursalCoincide(NormalizarTexto(x.Sucursal?.Nombre), sucursalesEsperadas))
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private static bool SucursalCoincide(string nombreSucursal, IReadOnlyCollection<string> sucursalesEsperadas)
    {
        return sucursalesEsperadas.Any(sucursalEsperada =>
            string.Equals(nombreSucursal, sucursalEsperada, StringComparison.Ordinal) ||
            nombreSucursal.Contains(sucursalEsperada, StringComparison.Ordinal));
    }

    private static string NormalizarTexto(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var textoNormalizado = valor.Trim().Normalize(NormalizationForm.FormD);
        var constructor = new StringBuilder(textoNormalizado.Length);
        foreach (var caracter in textoNormalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
            {
                constructor.Append(char.ToUpperInvariant(caracter));
            }
        }

        return constructor.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string ConstruirResumenTasas(IEnumerable<ElectronicaVallarta.Dominio.Entidades.TasaCambioRango> tasas)
    {
        return string.Join(" | ", tasas.Select(x =>
            $"Id={x.Id}; Pais={x.Pais?.Nombre}; Sucursal={x.Sucursal?.Nombre}; Rango={x.MontoDesdeUsd}-{x.MontoHastaUsd}; Tasa={x.TasaCambio.ToString("0.00", CultureInfo.InvariantCulture)}"));
    }

    private static void GuardarSvg(string rutaSvg, XDocument documento)
    {
        var atributos = File.GetAttributes(rutaSvg);
        if (atributos.HasFlag(FileAttributes.ReadOnly))
        {
            File.SetAttributes(rutaSvg, atributos & ~FileAttributes.ReadOnly);
        }

        using var flujo = new FileStream(rutaSvg, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024);
        documento.Save(flujo, SaveOptions.DisableFormatting);
        flujo.Flush(true);
    }

    private static ResultadoVerificacionSvg VerificarSvgGuardado(string rutaSvg, IReadOnlyDictionary<string, string> valoresEsperados)
    {
        using var flujo = new FileStream(rutaSvg, FileMode.Open, FileAccess.Read, FileShare.Read);
        var documento = XDocument.Load(flujo, LoadOptions.PreserveWhitespace);

        foreach (var valorEsperado in valoresEsperados)
        {
            var nodo = documento.Descendants().FirstOrDefault(x => string.Equals((string?)x.Attribute("id"), valorEsperado.Key, StringComparison.Ordinal));
            if (nodo is null)
            {
                return new ResultadoVerificacionSvg(false, $"No se encontro el nodo {valorEsperado.Key} despues de guardar.");
            }

            if (!string.Equals(nodo.Value, valorEsperado.Value, StringComparison.Ordinal))
            {
                return new ResultadoVerificacionSvg(false, $"El nodo {valorEsperado.Key} quedo con valor {nodo.Value} y se esperaba {valorEsperado.Value}.");
            }
        }

        return new ResultadoVerificacionSvg(true, "OK");
    }

    private sealed record ConfiguracionTasaSvg(string NodoId, string PaisNombre, IReadOnlyCollection<string> SucursalesEsperadas, decimal MontoDesdeUsd, decimal? MontoHastaUsd);
    private sealed record ResultadoVerificacionSvg(bool EsValida, string Detalle);
}
