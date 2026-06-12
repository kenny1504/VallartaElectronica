using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Datos.Inicializacion;
using ElectronicaVallarta.Extensiones;
using ElectronicaVallarta.Infraestructura.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
const long TamanoMaximoCargaBytes = 100 * 1024 * 1024;

var mvcBuilder = builder.Services.AddControllersWithViews();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/acceso";
        options.AccessDeniedPath = "/acceso";
        options.Cookie.Name = "ElectronicaVallarta.Admin";
        options.SlidingExpiration = true;
    });
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = TamanoMaximoCargaBytes;
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = TamanoMaximoCargaBytes;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = TamanoMaximoCargaBytes;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = 64 * 1024;
});
builder.Services.AddDbContext<ContextoAplicacion>(options =>
    options
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .UseSqlServer(
            builder.Configuration.GetConnectionString("ConexionSqlServer"),
            sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
builder.Services.AgregarDependenciasAplicacion();
builder.Services.AgregarLoggingBaseDatos();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

using (var alcance = app.Services.CreateScope())
{
    var inicializador = alcance.ServiceProvider.GetRequiredService<InicializadorDatos>();
    await inicializador.InicializarAsync();
}

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.Use(async (contexto, siguiente) =>
{
    var archivosSvgTasas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "tasas.svg",
        "tasas-post.svg",
        "tasa-guate.svg"
    };
    var nombreArchivoSvg = Path.GetFileName(contexto.Request.Path.Value) ?? string.Empty;
    var esSvgTasas = contexto.Request.Path.StartsWithSegments("/uploads/publicidad") &&
        !string.IsNullOrWhiteSpace(nombreArchivoSvg) &&
        archivosSvgTasas.Contains(nombreArchivoSvg);

    if (!esSvgTasas)
    {
        await siguiente();
        return;
    }

    var ambiente = contexto.RequestServices.GetRequiredService<IWebHostEnvironment>();
    var logger = contexto.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("PublicidadSvgEndpoint");
    var rutaSvg = Path.Combine(ambiente.WebRootPath, "uploads", "publicidad", nombreArchivoSvg);

    contexto.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
    contexto.Response.Headers.Pragma = "no-cache";
    contexto.Response.Headers.Expires = "0";
    contexto.Response.Headers.ETag = $"\"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\"";
    contexto.Response.Headers["X-Tasas-Svg-File"] = nombreArchivoSvg;
    contexto.Response.Headers["X-Tasas-Svg-Source"] = rutaSvg;

    if (!File.Exists(rutaSvg))
    {
        logger.LogError("Solicitud publica de {NombreArchivoSvg} fallida. No se encontro el archivo en {RutaSvg}.", nombreArchivoSvg, rutaSvg);
        contexto.Response.StatusCode = StatusCodes.Status404NotFound;
        await contexto.Response.WriteAsync($"No se encontro el archivo {nombreArchivoSvg}.");
        return;
    }

    var informacionArchivo = new FileInfo(rutaSvg);
    contexto.Response.Headers["X-Tasas-Svg-LastWriteUtc"] = informacionArchivo.LastWriteTimeUtc.ToString("O");
    contexto.Response.Headers["X-Tasas-Svg-Length"] = informacionArchivo.Length.ToString(CultureInfo.InvariantCulture);
    contexto.Response.ContentType = "image/svg+xml";
    contexto.Response.ContentLength = informacionArchivo.Length;

    logger.LogInformation(
        "Sirviendo {NombreArchivoSvg} desde endpoint dinamico. RutaSvg: {RutaSvg}. UltimaEscrituraUtc: {UltimaEscrituraUtc}. TamanoBytes: {TamanoBytes}.",
        nombreArchivoSvg,
        rutaSvg,
        informacionArchivo.LastWriteTimeUtc,
        informacionArchivo.Length);

    await contexto.Response.SendFileAsync(rutaSvg);
});
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = contexto =>
    {
        if (!contexto.Context.Request.Path.StartsWithSegments("/uploads/publicidad"))
        {
            return;
        }

        contexto.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        contexto.Context.Response.Headers.Pragma = "no-cache";
        contexto.Context.Response.Headers.Expires = "0";
    }
});
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();

app.Run();
