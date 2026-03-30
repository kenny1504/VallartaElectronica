using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioSucursal
{
    Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesAsync();
    Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesActivasPorPaisAsync(int paisId);
    Task<Sucursal?> ObtenerSucursalPorIdAsync(int id, bool soloLectura = true);
    Task CrearAsync(Sucursal sucursal);
    Task ActualizarAsync(Sucursal sucursal);
    Task EliminarAsync(int id);
}
