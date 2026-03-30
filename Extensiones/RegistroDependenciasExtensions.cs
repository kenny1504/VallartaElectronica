using ElectronicaVallarta.Datos.Inicializacion;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Repositorios;
using ElectronicaVallarta.Servicios;

namespace ElectronicaVallarta.Extensiones;

public static class RegistroDependenciasExtensions
{
    /// <summary>
    /// Registra las dependencias de los servicios y repositorios requeridos para la aplicación
    /// en el contenedor de inyección de dependencias.
    /// </summary>
    /// <param name="services">La colección de servicios donde se agregarán las dependencias.</param>
    /// <returns>La colección de servicios con las dependencias agregadas.</returns>
    public static IServiceCollection AgregarDependenciasAplicacion(this IServiceCollection services)
    {
        services.AddScoped<IRepositorioPais, RepositorioPais>();
        services.AddScoped<IRepositorioSucursal, RepositorioSucursal>();
        services.AddScoped<IRepositorioTasaCambio, RepositorioTasaCambio>();
        services.AddScoped<IRepositorioUsuarioAdministrador, RepositorioUsuarioAdministrador>();
        services.AddScoped<IServicioPais, ServicioPais>();
        services.AddScoped<IServicioSucursal, ServicioSucursal>();
        services.AddScoped<IServicioTasaCambio, ServicioTasaCambio>();
        services.AddScoped<IServicioCalculadora, ServicioCalculadora>();
        services.AddScoped<IServicioAutenticacionAdministrador, ServicioAutenticacionAdministrador>();
        services.AddScoped<InicializadorDatos>();

        return services;
    }
}
