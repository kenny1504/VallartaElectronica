using ElectronicaVallarta.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Datos;

public class ContextoAplicacion(DbContextOptions<ContextoAplicacion> options) : DbContext(options)
{
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<TasaCambioRango> TasasCambioRango => Set<TasaCambioRango>();
    public DbSet<UsuarioAdministrador> UsuariosAdministradores => Set<UsuarioAdministrador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContextoAplicacion).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
