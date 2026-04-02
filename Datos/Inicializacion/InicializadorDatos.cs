using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Servicios;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Datos.Inicializacion;

public class InicializadorDatos(ContextoAplicacion contexto, ILogger<InicializadorDatos> logger)
{
    public async Task InicializarAsync()
    {
        await contexto.Database.MigrateAsync();
        var fechaCreacion = DateTime.UtcNow;

        if (!await contexto.UsuariosAdministradores.AnyAsync())
        {
            await contexto.UsuariosAdministradores.AddAsync(new UsuarioAdministrador
            {
                NombreUsuario = "admin",
                NombreCompleto = "Administrador general",
                ClaveHash = ServicioHashClave.GenerarHash("Admin123!"),
                EstaActivo = true,
                FechaCreacion = fechaCreacion
            });
            await contexto.SaveChangesAsync();
            logger.LogInformation("Se creo el usuario administrador inicial.");
        }

        if (await contexto.Paises.AnyAsync())
        {
            return;
        }

        var fechaHoy = DateTime.Today;
        var mexico = new Pais { Nombre = "Mexico", CodigoMoneda = "MXN", SimboloMoneda = "$", FechaCreacion = fechaCreacion };
        await contexto.Paises.AddRangeAsync(mexico);
        await contexto.SaveChangesAsync();

        var coppel = new Sucursal { Nombre = "BanCoppel", PaisId = mexico.Id, FechaCreacion = fechaCreacion };
        var elektra = new Sucursal { Nombre = "Elektra", PaisId = mexico.Id, FechaCreacion = fechaCreacion };

        await contexto.Sucursales.AddRangeAsync(coppel, elektra);
        await contexto.SaveChangesAsync();

        await contexto.TasasCambioRango.AddRangeAsync(
            new TasaCambioRango { PaisId = mexico.Id, SucursalId = coppel.Id, MontoDesdeUsd = 1, MontoHastaUsd = 999.99m, TasaCambio = 17.70m, FechaTasa = fechaHoy, FechaCreacion = fechaCreacion },
            new TasaCambioRango { PaisId = mexico.Id, SucursalId = coppel.Id, MontoDesdeUsd = 1000, MontoHastaUsd = null, TasaCambio = 17.00m, FechaTasa = fechaHoy, FechaCreacion = fechaCreacion },
            new TasaCambioRango { PaisId = mexico.Id, SucursalId = elektra.Id, MontoDesdeUsd = 1, MontoHastaUsd = 999.99m, TasaCambio = 17.55m, FechaTasa = fechaHoy, FechaCreacion = fechaCreacion },
            new TasaCambioRango { PaisId = mexico.Id, SucursalId = elektra.Id, MontoDesdeUsd = 1000m, MontoHastaUsd = null, TasaCambio = 18.11m, FechaTasa = fechaHoy, FechaCreacion = fechaCreacion }
        );

        await contexto.SaveChangesAsync();
    }
}
