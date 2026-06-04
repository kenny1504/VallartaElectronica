using System.Text.Json;
using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Infraestructura.Logging;

public sealed class ProveedorLogBaseDatos(
    IServiceScopeFactory scopeFactory,
    IHttpContextAccessor httpContextAccessor,
    IHostEnvironment ambiente) : ILoggerProvider
{
    private static readonly AsyncLocal<bool> GuardandoLog = new();

    public ILogger CreateLogger(string categoryName) =>
        new LoggerBaseDatos(categoryName, scopeFactory, httpContextAccessor, ambiente, GuardandoLog);

    public void Dispose()
    {
    }

    private sealed class LoggerBaseDatos(
        string categoria,
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        IHostEnvironment ambiente,
        AsyncLocal<bool> guardandoLog) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= LogLevel.Information &&
            logLevel != LogLevel.None &&
            !categoria.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) &&
            !categoria.StartsWith("ElectronicaVallarta.Infraestructura.Logging", StringComparison.OrdinalIgnoreCase);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel) || guardandoLog.Value)
            {
                return;
            }

            var mensaje = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(mensaje) && exception is null)
            {
                return;
            }

            var contextoHttp = httpContextAccessor.HttpContext;
            var registro = new RegistroLogAplicacion
            {
                FechaUtc = DateTime.UtcNow,
                Nivel = logLevel.ToString(),
                Categoria = Recortar(categoria, 256) ?? string.Empty,
                Mensaje = Recortar(mensaje, 4000) ?? string.Empty,
                DetalleExcepcion = exception?.ToString(),
                Propiedades = SerializarPropiedades(state, eventId),
                Ruta = Recortar(contextoHttp?.Request.Path.Value, 512),
                MetodoHttp = Recortar(contextoHttp?.Request.Method, 12),
                Usuario = Recortar(contextoHttp?.User.Identity?.Name, 120),
                TraceIdentifier = Recortar(contextoHttp?.TraceIdentifier, 128),
                Ambiente = Recortar(ambiente.EnvironmentName, 80)
            };

            _ = Task.Run(() => GuardarAsync(registro));
        }

        private async Task GuardarAsync(RegistroLogAplicacion registro)
        {
            try
            {
                guardandoLog.Value = true;
                using var alcance = scopeFactory.CreateScope();
                var contexto = alcance.ServiceProvider.GetRequiredService<ContextoAplicacion>();
                contexto.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                await contexto.RegistrosLogsAplicacion.AddAsync(registro);
                await contexto.SaveChangesAsync();
            }
            catch
            {
                // El proveedor de logs no relanza errores para no romper el flujo principal.
            }
            finally
            {
                guardandoLog.Value = false;
            }
        }

        private static string? SerializarPropiedades<TState>(TState state, EventId eventId)
        {
            var propiedades = new Dictionary<string, object?>
            {
                ["EventId"] = eventId.Id,
                ["EventName"] = eventId.Name
            };

            if (state is IEnumerable<KeyValuePair<string, object?>> pares)
            {
                foreach (var par in pares)
                {
                    if (par.Key == "{OriginalFormat}")
                    {
                        continue;
                    }

                    propiedades[par.Key] = par.Value?.ToString();
                }
            }

            return propiedades.Count == 0 ? null : JsonSerializer.Serialize(propiedades);
        }

        private static string? Recortar(string? valor, int longitudMaxima) =>
            string.IsNullOrEmpty(valor)
                ? valor
                : valor.Length <= longitudMaxima ? valor : valor[..longitudMaxima];
    }
}
