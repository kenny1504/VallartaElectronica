using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ElectronicaVallarta.Datos;

public class FactoriaContextoAplicacion : IDesignTimeDbContextFactory<ContextoAplicacion>
{
    public ContextoAplicacion CreateDbContext(string[] args)
    {
        var configuracion = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cadenaConexion = configuracion.GetConnectionString("ConexionSqlServer")
                              ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'ConexionSqlServer'.");

        var opcionesBuilder = new DbContextOptionsBuilder<ContextoAplicacion>();
        opcionesBuilder.UseSqlServer(cadenaConexion);

        return new ContextoAplicacion(opcionesBuilder.Options);
    }
}
