using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Dominio.Enumeraciones;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos.Dto;
using System.Globalization;

namespace ElectronicaVallarta.Servicios;

public class ServicioPublicidad(IRepositorioPublicidad repositorioPublicidad, IWebHostEnvironment ambiente) : IServicioPublicidad
{
    private static readonly HashSet<string> ExtensionesImagen = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg"
    };

    private static readonly HashSet<string> ExtensionesVideo = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".mov", ".m4v", ".avi", ".mpeg", ".mpg"
    };

    private static readonly HashSet<string> TiposContenidoSvg = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/svg+xml", "application/svg+xml", "application/xml", "text/xml", "text/plain", "application/octet-stream"
    };

    private const long TamanoMaximoArchivoBytes = 100 * 1024 * 1024;
    private const string RutaPublicaBase = "/uploads/publicidad";

    public Task<IReadOnlyCollection<Publicidad>> ObtenerPublicidadesAsync() => repositorioPublicidad.ObtenerTodasAsync();

    public async Task<IReadOnlyCollection<PublicidadActivaDto>> ObtenerPublicidadesActivasAsync(DateTime fechaActual)
    {
        var publicidades = await repositorioPublicidad.ObtenerActivasVigentesAsync(fechaActual);
        return publicidades.Select(x => new PublicidadActivaDto
        {
            Id = x.Id,
            Titulo = x.Titulo,
            Descripcion = x.Descripcion,
            TipoRecurso = x.TipoRecurso.ToString(),
            UrlRecurso = ConstruirUrlRecursoVersionada(x.UrlRecurso),
            DuracionSegundos = x.DuracionSegundos,
            Orden = x.Orden
        }).ToList();
    }

    public Task<Publicidad?> ObtenerPublicidadPorIdAsync(int id, bool soloLectura = true) => repositorioPublicidad.ObtenerPorIdAsync(id, soloLectura);

    public async Task CrearAsync(Publicidad publicidad, IFormFile? archivo)
    {
        ValidarPublicidad(publicidad);
        if (archivo is null || archivo.Length == 0)
        {
            throw new InvalidOperationException("Selecciona un archivo de imagen o video.");
        }

        publicidad.Titulo = publicidad.Titulo.Trim();
        publicidad.Descripcion = string.IsNullOrWhiteSpace(publicidad.Descripcion) ? null : publicidad.Descripcion.Trim();
        publicidad.UrlRecurso = await GuardarArchivoAsync(archivo, publicidad.TipoRecurso);
        publicidad.FechaCreacion = DateTime.UtcNow;
        await repositorioPublicidad.AgregarAsync(publicidad);
    }

    public async Task ActualizarAsync(Publicidad publicidad, IFormFile? archivo)
    {
        ValidarPublicidad(publicidad);
        var publicidadActual = await repositorioPublicidad.ObtenerPorIdAsync(publicidad.Id, false)
                                ?? throw new InvalidOperationException("La publicidad solicitada no existe.");

        var rutaAnterior = publicidadActual.UrlRecurso;
        publicidadActual.Titulo = publicidad.Titulo.Trim();
        publicidadActual.Descripcion = string.IsNullOrWhiteSpace(publicidad.Descripcion) ? null : publicidad.Descripcion.Trim();
        publicidadActual.TipoRecurso = publicidad.TipoRecurso;
        publicidadActual.DuracionSegundos = publicidad.DuracionSegundos;
        publicidadActual.Orden = publicidad.Orden;
        publicidadActual.EstaActivo = publicidad.EstaActivo;
        publicidadActual.FechaInicio = publicidad.FechaInicio;
        publicidadActual.FechaFin = publicidad.FechaFin;
        publicidadActual.FechaActualizacion = DateTime.UtcNow;

        if (archivo is not null && archivo.Length > 0)
        {
            publicidadActual.UrlRecurso = await GuardarArchivoAsync(archivo, publicidad.TipoRecurso);
        }

        await repositorioPublicidad.ActualizarAsync(publicidadActual);

        if (!string.Equals(rutaAnterior, publicidadActual.UrlRecurso, StringComparison.OrdinalIgnoreCase))
        {
            EliminarArchivoFisico(rutaAnterior);
        }
    }

    public async Task CambiarEstadoAsync(int id)
    {
        var publicidad = await repositorioPublicidad.ObtenerPorIdAsync(id, false)
                         ?? throw new InvalidOperationException("La publicidad solicitada no existe.");

        publicidad.EstaActivo = !publicidad.EstaActivo;
        publicidad.FechaActualizacion = DateTime.UtcNow;
        await repositorioPublicidad.ActualizarAsync(publicidad);
    }

    public async Task EliminarAsync(int id)
    {
        var publicidad = await repositorioPublicidad.ObtenerPorIdAsync(id, false)
                         ?? throw new InvalidOperationException("La publicidad solicitada no existe.");
        var rutaArchivo = publicidad.UrlRecurso;

        await repositorioPublicidad.EliminarAsync(publicidad);
        EliminarArchivoFisico(rutaArchivo);
    }

    private static void ValidarPublicidad(Publicidad publicidad)
    {
        if (string.IsNullOrWhiteSpace(publicidad.Titulo))
        {
            throw new InvalidOperationException("El titulo es obligatorio.");
        }

        if (publicidad.DuracionSegundos <= 0)
        {
            throw new InvalidOperationException("La duracion debe ser mayor a cero segundos.");
        }

        if (publicidad.FechaInicio.HasValue && publicidad.FechaFin.HasValue && publicidad.FechaFin.Value < publicidad.FechaInicio.Value)
        {
            throw new InvalidOperationException("La fecha fin no puede ser menor que la fecha inicio.");
        }
    }

    private async Task<string> GuardarArchivoAsync(IFormFile archivo, TipoRecursoPublicidad tipoRecurso)
    {
        var extension = Path.GetExtension(archivo.FileName);
        if (!EsExtensionPermitida(tipoRecurso, extension))
        {
            throw new InvalidOperationException("El tipo de archivo no esta permitido para el recurso seleccionado.");
        }

        if (archivo.Length > TamanoMaximoArchivoBytes)
        {
            throw new InvalidOperationException("El archivo no puede superar 100 MB.");
        }

        if (!EsContenidoEsperado(archivo, tipoRecurso, extension))
        {
            throw new InvalidOperationException("El contenido del archivo no coincide con el tipo de recurso seleccionado.");
        }

        var directorio = Path.Combine(ambiente.WebRootPath, "uploads", "publicidad");
        Directory.CreateDirectory(directorio);

        var nombreArchivo = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var rutaFisica = Path.Combine(directorio, nombreArchivo);
        await using var flujo = File.Create(rutaFisica);
        await archivo.CopyToAsync(flujo);

        return $"{RutaPublicaBase}/{nombreArchivo}";
    }

    private string ConstruirUrlRecursoVersionada(string urlRecurso)
    {
        if (string.IsNullOrWhiteSpace(urlRecurso) || !urlRecurso.StartsWith(RutaPublicaBase, StringComparison.OrdinalIgnoreCase))
        {
            return urlRecurso;
        }

        var rutaRelativa = urlRecurso.Split('?', '#')[0].TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var rutaFisica = Path.Combine(ambiente.WebRootPath, rutaRelativa);
        if (!File.Exists(rutaFisica))
        {
            return urlRecurso;
        }

        var informacionArchivo = new FileInfo(rutaFisica);
        var version = string.Create(CultureInfo.InvariantCulture, $"{informacionArchivo.LastWriteTimeUtc.Ticks}-{informacionArchivo.Length}");
        var separador = urlRecurso.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{urlRecurso}{separador}v={version}";
    }

    private static bool EsExtensionPermitida(TipoRecursoPublicidad tipoRecurso, string extension) =>
        tipoRecurso == TipoRecursoPublicidad.Imagen
            ? ExtensionesImagen.Contains(extension)
            : ExtensionesVideo.Contains(extension);

    private static bool EsContenidoEsperado(IFormFile archivo, TipoRecursoPublicidad tipoRecurso, string extension)
    {
        if (string.IsNullOrWhiteSpace(archivo.ContentType))
        {
            return true;
        }

        if (tipoRecurso == TipoRecursoPublicidad.Imagen && extension.Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return TiposContenidoSvg.Contains(archivo.ContentType);
        }

        return tipoRecurso == TipoRecursoPublicidad.Imagen
            ? archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            : archivo.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    }

    private void EliminarArchivoFisico(string? rutaPublica)
    {
        if (string.IsNullOrWhiteSpace(rutaPublica) || !rutaPublica.StartsWith(RutaPublicaBase, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var rutaRelativa = rutaPublica.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var rutaFisica = Path.Combine(ambiente.WebRootPath, rutaRelativa);
        if (File.Exists(rutaFisica))
        {
            File.Delete(rutaFisica);
        }
    }
}
