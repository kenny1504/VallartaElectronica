using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;

namespace ElectronicaVallarta.Servicios;

public class ServicioActualizadorPublicidadSvg(
    IRepositorioTasaCambio repositorioTasaCambio,
    IWebHostEnvironment entorno,
    ILogger<ServicioActualizadorPublicidadSvg> logger) : IServicioActualizadorPublicidadSvg
{
    private static readonly IReadOnlyCollection<ConfiguracionTasaSvg> ConfiguracionesMexico =
    [
        new("rate_menos1000_elektra", 1, "Mexico", 2, ["Elektra", "Banco Azteca"], 1, 1000),
        new("rate_mas1000_elektra", 1, "Mexico", 2, ["Elektra", "Banco Azteca"], 1001, 2999),
        new("rate_menos1000_bancoppel", 1, "Mexico", 1, ["BanCoppel"], 1, 1000),
        new("rate_mas1000_bancoppel", 1, "Mexico", 1, ["BanCoppel"], 1001, 2999),
        new("rate_menos1000_deposito", 1, "Mexico", 3, ["Deposito a cuenta", "Deposito"], 1, 1000),
        new("rate_mas1000_deposito", 1, "Mexico", 3, ["Deposito a cuenta", "Deposito"], 1001, 2999)
    ];

    private static readonly IReadOnlyDictionary<string, AtributosTextoSvg> AtributosTextoHistoria =
        new Dictionary<string, AtributosTextoSvg>(StringComparer.Ordinal)
        {
            ["rate_menos1000_elektra"] = new("626", "56"),
            ["rate_mas1000_elektra"] = new("626", "56"),
            ["rate_menos1000_bancoppel"] = new("765", "56"),
            ["rate_mas1000_bancoppel"] = new("765", "56"),
            ["rate_menos1000_deposito"] = new("905", "56"),
            ["rate_mas1000_deposito"] = new("905", "56")
        };

    private static readonly IReadOnlyCollection<ArchivoSvgPublicidad> ArchivosSvg =
    [
        new("uploads/publicidad/tasas.svg", ConfiguracionesMexico),
        new("uploads/publicidad/tasas-post.svg", ConfiguracionesMexico, AtributosTextoHistoria)
    ];

    public async Task<ResultadoActualizacionPublicidadSvg> ActualizarAsync(DateTime fechaTasa)
    {
        var fecha = fechaTasa.Date;
        var archivos = ArchivosSvg
            .Select(archivo => archivo with { RutaFisica = Path.Combine(entorno.WebRootPath, archivo.RutaRelativa) })
            .ToList();

        try
        {
            RegistrarDiagnosticoInicial(archivos);

            var archivosFaltantes = archivos.Where(archivo => !File.Exists(archivo.RutaFisica)).ToList();
            if (archivosFaltantes.Count > 0)
            {
                logger.LogError(
                    "No se encontraron todos los archivos SVG requeridos para actualizar publicidad. ArchivosFaltantes: {ArchivosFaltantes}.",
                    string.Join(", ", archivosFaltantes.Select(x => x.RutaFisica)));
                return new ResultadoActualizacionPublicidadSvg(false, "No se encontraron todos los archivos SVG requeridos para actualizar la publicidad.");
            }

            var tasasFecha = await repositorioTasaCambio.ObtenerTodosAsync(fecha);
            var totalTasasActualizadas = 0;

            foreach (var archivo in archivos)
            {
                var valoresEsperados = ObtenerValoresEsperados(tasasFecha, archivo.Configuraciones, fecha, archivo.RutaRelativa);
                if (valoresEsperados.Count == 0)
                {
                    return new ResultadoActualizacionPublicidadSvg(false, $"No se encontro ninguna tasa vigente para actualizar {archivo.RutaRelativa} del {fecha:MM/dd/yyyy}.");
                }

                var resultadoArchivo = ActualizarArchivoSvg(archivo, valoresEsperados);
                if (!resultadoArchivo.EsValido)
                {
                    return new ResultadoActualizacionPublicidadSvg(false, resultadoArchivo.Mensaje);
                }

                totalTasasActualizadas += valoresEsperados.Count;
            }

            logger.LogInformation(
                "Archivos SVG de publicidad actualizados y verificados correctamente. FechaTasa: {FechaTasa}. Archivos: {Archivos}. TotalTasasActualizadas: {TotalTasasActualizadas}.",
                fecha,
                string.Join(", ", archivos.Select(x => x.RutaFisica)),
                totalTasasActualizadas);

            return new ResultadoActualizacionPublicidadSvg(true, "Publicidad e historia actualizadas correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "No se pudieron actualizar los archivos SVG de publicidad. RutasSvg: {RutasSvg}.",
                string.Join(", ", archivos.Select(x => x.RutaFisica)));
            return new ResultadoActualizacionPublicidadSvg(false, "No se pudo actualizar la publicidad SVG. Intentalo nuevamente.");
        }
    }

    private void RegistrarDiagnosticoInicial(IEnumerable<ArchivoSvgPublicidad> archivos)
    {
        foreach (var archivo in archivos)
        {
            logger.LogInformation(
                "Iniciando actualizacion de archivo SVG. WebRootPath: {WebRootPath}. RutaRelativaSvg: {RutaRelativaSvg}. RutaSvg: {RutaSvg}. DirectorioExiste: {DirectorioExiste}. ArchivoExiste: {ArchivoExiste}. AtributosArchivo: {AtributosArchivo}. TamanoBytes: {TamanoBytes}. UsuarioProceso: {UsuarioProceso}.",
                entorno.WebRootPath,
                archivo.RutaRelativa,
                archivo.RutaFisica,
                Directory.Exists(Path.GetDirectoryName(archivo.RutaFisica)),
                File.Exists(archivo.RutaFisica),
                File.Exists(archivo.RutaFisica) ? File.GetAttributes(archivo.RutaFisica).ToString() : "No existe",
                File.Exists(archivo.RutaFisica) ? new FileInfo(archivo.RutaFisica).Length : 0,
                Environment.UserName);
        }
    }

    private Dictionary<string, string> ObtenerValoresEsperados(
        IEnumerable<TasaCambioRango> tasasFecha,
        IEnumerable<ConfiguracionTasaSvg> configuraciones,
        DateTime fecha,
        string rutaRelativa)
    {
        var tasas = tasasFecha.ToList();
        var valoresEsperados = new Dictionary<string, string>();

        foreach (var configuracion in configuraciones)
        {
            var tasa = ObtenerTasaConfigurada(tasas, configuracion);

            if (tasa is null)
            {
                logger.LogWarning(
                    "No se encontro tasa vigente para el nodo SVG {NodoId}. Archivo: {Archivo}. FechaTasa: {FechaTasa}, PaisId: {PaisId}, Pais: {Pais}, SucursalId: {SucursalId}, SucursalesEsperadas: {SucursalesEsperadas}, MontoDesdeUsd: {MontoDesdeUsd}, MontoHastaUsd: {MontoHastaUsd}. TasasDisponibles: {TasasDisponibles}.",
                    configuracion.NodoId,
                    rutaRelativa,
                    fecha,
                    configuracion.PaisId,
                    configuracion.PaisNombre,
                    configuracion.SucursalId,
                    string.Join(", ", configuracion.SucursalesEsperadas),
                    configuracion.MontoDesdeUsd,
                    configuracion.MontoHastaUsd,
                    ConstruirResumenTasas(tasas));
                continue;
            }

            var valorTasa = tasa.TasaCambio.ToString("0.00", CultureInfo.InvariantCulture);
            valoresEsperados[configuracion.NodoId] = valorTasa;

            logger.LogInformation(
                "Nodo SVG {NodoId} usara TasaCambioRangoId {TasaCambioRangoId}. Archivo: {Archivo}. PaisId: {PaisId}. Pais: {Pais}. SucursalId: {SucursalId}. Sucursal: {Sucursal}. Rango: {MontoDesdeUsd}-{MontoHastaUsd}. Valor: {Valor}.",
                configuracion.NodoId,
                tasa.Id,
                rutaRelativa,
                tasa.PaisId,
                tasa.Pais?.Nombre,
                tasa.SucursalId,
                tasa.Sucursal?.Nombre,
                tasa.MontoDesdeUsd,
                tasa.MontoHastaUsd,
                valorTasa);
        }

        return valoresEsperados;
    }

    private ResultadoActualizacionArchivoSvg ActualizarArchivoSvg(ArchivoSvgPublicidad archivo, IReadOnlyDictionary<string, string> valoresEsperados)
    {
        var documento = XDocument.Load(archivo.RutaFisica, LoadOptions.PreserveWhitespace);
        var tasasActualizadas = 0;

        foreach (var valorEsperado in valoresEsperados)
        {
            var nodo = documento.Descendants().FirstOrDefault(x => string.Equals((string?)x.Attribute("id"), valorEsperado.Key, StringComparison.Ordinal));
            if (nodo is null)
            {
                logger.LogWarning(
                    "No se encontro el nodo SVG con id {NodoId} en el archivo {RutaSvg}.",
                    valorEsperado.Key,
                    archivo.RutaFisica);
                continue;
            }

            AplicarAtributosTexto(archivo, valorEsperado.Key, nodo);
            nodo.Value = valorEsperado.Value;
            tasasActualizadas++;
        }

        if (tasasActualizadas == 0)
        {
            return new ResultadoActualizacionArchivoSvg(false, $"No se encontro ningun nodo actualizable en el archivo {archivo.RutaRelativa}.");
        }

        GuardarSvg(archivo.RutaFisica, documento);
        var verificacion = VerificarSvgGuardado(archivo.RutaFisica, valoresEsperados);
        if (!verificacion.EsValida)
        {
            logger.LogError(
                "El SVG fue procesado pero no se pudo confirmar la escritura en disco. RutaSvg: {RutaSvg}. Detalle: {Detalle}.",
                archivo.RutaFisica,
                verificacion.Detalle);
            return new ResultadoActualizacionArchivoSvg(false, $"El servidor no pudo confirmar que el archivo {archivo.RutaRelativa} quedara actualizado. Revisa permisos de escritura sobre wwwroot/uploads/publicidad.");
        }

        var fechaActualizacion = File.GetLastWriteTimeUtc(archivo.RutaFisica);
        logger.LogInformation(
            "Archivo SVG actualizado y verificado correctamente. RutaSvg: {RutaSvg}. TasasActualizadas: {TasasActualizadas}. UltimaEscrituraUtc: {UltimaEscrituraUtc}.",
            archivo.RutaFisica,
            tasasActualizadas,
            fechaActualizacion);

        return new ResultadoActualizacionArchivoSvg(true, "OK");
    }

    private static void AplicarAtributosTexto(ArchivoSvgPublicidad archivo, string nodoId, XElement nodo)
    {
        if (archivo.AtributosTextoPorNodo is null ||
            !archivo.AtributosTextoPorNodo.TryGetValue(nodoId, out var atributos))
        {
            return;
        }

        nodo.SetAttributeValue("y", atributos.Y);
        nodo.SetAttributeValue("font-family", "Arial, Helvetica, sans-serif");
        nodo.SetAttributeValue("font-size", atributos.FontSize);
        nodo.SetAttributeValue("font-weight", "900");
        nodo.SetAttributeValue("fill", "#33475b");
        nodo.SetAttributeValue("text-anchor", "middle");
        nodo.SetAttributeValue("dominant-baseline", "middle");
    }

    private static TasaCambioRango? ObtenerTasaConfigurada(
        IEnumerable<TasaCambioRango> tasas,
        ConfiguracionTasaSvg configuracion)
    {
        var paisEsperado = NormalizarTexto(configuracion.PaisNombre);
        var sucursalesEsperadas = configuracion.SucursalesEsperadas.Select(NormalizarTexto).ToList();

        return tasas
            .Where(x => x.EstaActivo &&
                        PaisCoincide(x, configuracion, paisEsperado) &&
                        SucursalCoincide(x, configuracion, sucursalesEsperadas) &&
                        RangoCoincide(x, configuracion))
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private static bool PaisCoincide(TasaCambioRango tasa, ConfiguracionTasaSvg configuracion, string paisEsperado)
    {
        if (configuracion.PaisId.HasValue)
        {
            return tasa.PaisId == configuracion.PaisId.Value;
        }

        return string.Equals(NormalizarTexto(tasa.Pais?.Nombre), paisEsperado, StringComparison.Ordinal);
    }

    private static bool SucursalCoincide(
        TasaCambioRango tasa,
        ConfiguracionTasaSvg configuracion,
        IReadOnlyCollection<string> sucursalesEsperadas)
    {
        if (configuracion.SucursalId.HasValue)
        {
            return tasa.SucursalId == configuracion.SucursalId.Value;
        }

        var nombreSucursal = NormalizarTexto(tasa.Sucursal?.Nombre);
        return sucursalesEsperadas.Any(sucursalEsperada =>
            string.Equals(nombreSucursal, sucursalEsperada, StringComparison.Ordinal) ||
            nombreSucursal.Contains(sucursalEsperada, StringComparison.Ordinal));
    }

    private static bool RangoCoincide(TasaCambioRango tasa, ConfiguracionTasaSvg configuracion)
    {
        if (!configuracion.MontoDesdeUsd.HasValue && !configuracion.MontoHastaUsd.HasValue)
        {
            return true;
        }

        return tasa.MontoDesdeUsd == configuracion.MontoDesdeUsd &&
            tasa.MontoHastaUsd == configuracion.MontoHastaUsd;
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

    private static string ConstruirResumenTasas(IEnumerable<TasaCambioRango> tasas)
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

    private sealed record ConfiguracionTasaSvg(
        string NodoId,
        int? PaisId,
        string PaisNombre,
        int? SucursalId,
        IReadOnlyCollection<string> SucursalesEsperadas,
        decimal? MontoDesdeUsd,
        decimal? MontoHastaUsd);

    private sealed record ArchivoSvgPublicidad(
        string RutaRelativa,
        IReadOnlyCollection<ConfiguracionTasaSvg> Configuraciones,
        IReadOnlyDictionary<string, AtributosTextoSvg>? AtributosTextoPorNodo = null,
        string RutaFisica = "");

    private sealed record AtributosTextoSvg(string Y, string FontSize);
    private sealed record ResultadoActualizacionArchivoSvg(bool EsValido, string Mensaje);
    private sealed record ResultadoVerificacionSvg(bool EsValida, string Detalle);
}
