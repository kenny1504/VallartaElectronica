using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;

namespace ElectronicaVallarta.Servicios;

public class ServicioSucursal(IRepositorioSucursal repositorioSucursal, IRepositorioPais repositorioPais) : IServicioSucursal
{
    public Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesAsync() => repositorioSucursal.ObtenerTodosAsync();
    public Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesActivasAsync() => repositorioSucursal.ObtenerActivasAsync();
    public Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesActivasPorPaisAsync(int paisId) => repositorioSucursal.ObtenerActivasPorPaisAsync(paisId);
    public Task<Sucursal?> ObtenerSucursalPorIdAsync(int id, bool soloLectura = true) => repositorioSucursal.ObtenerPorIdAsync(id, soloLectura);

    public async Task CrearAsync(Sucursal sucursal)
    {
        await ValidarPaisAsync(sucursal.PaisId);
        if (await repositorioSucursal.ExisteNombreDuplicadoAsync(sucursal.PaisId, sucursal.Nombre))
        {
            throw new InvalidOperationException("Ya existe una sucursal o canal con ese nombre para el pais seleccionado.");
        }

        sucursal.Nombre = sucursal.Nombre.Trim();
        sucursal.FechaCreacion = DateTime.UtcNow;
        await repositorioSucursal.AgregarAsync(sucursal);
    }

    public async Task ActualizarAsync(Sucursal sucursal)
    {
        var sucursalActual = await repositorioSucursal.ObtenerPorIdAsync(sucursal.Id, false)
                            ?? throw new InvalidOperationException("La sucursal solicitada no existe.");

        await ValidarPaisAsync(sucursal.PaisId);
        if (await repositorioSucursal.ExisteNombreDuplicadoAsync(sucursal.PaisId, sucursal.Nombre, sucursal.Id))
        {
            throw new InvalidOperationException("Ya existe una sucursal o canal con ese nombre para el pais seleccionado.");
        }

        sucursalActual.PaisId = sucursal.PaisId;
        sucursalActual.Nombre = sucursal.Nombre.Trim();
        sucursalActual.EstaActivo = sucursal.EstaActivo;
        sucursalActual.FechaActualizacion = DateTime.UtcNow;
        await repositorioSucursal.ActualizarAsync(sucursalActual);
    }

    public async Task EliminarAsync(int id)
    {
        var sucursal = await repositorioSucursal.ObtenerPorIdAsync(id, false)
                       ?? throw new InvalidOperationException("La sucursal solicitada no existe.");

        await repositorioSucursal.EliminarAsync(sucursal);
    }

    private async Task ValidarPaisAsync(int paisId)
    {
        if (!await repositorioPais.ExisteActivoAsync(paisId))
        {
            throw new InvalidOperationException("El pais seleccionado no existe.");
        }
    }
}
