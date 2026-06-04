namespace ElectronicaVallarta.Infraestructura.Logging;

public static class LoggerBaseDatosExtensions
{
    public static IServiceCollection AgregarLoggingBaseDatos(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ILoggerProvider, ProveedorLogBaseDatos>();
        return services;
    }
}
