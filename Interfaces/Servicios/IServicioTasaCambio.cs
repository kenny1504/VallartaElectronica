using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioTasaCambio
{
    Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTasasAsync();
    Task<TasaCambioRango?> ObtenerTasaPorIdAsync(int id, bool soloLectura = true);
    Task CrearAsync(TasaCambioRango tasaCambioRango);
    Task ActualizarAsync(TasaCambioRango tasaCambioRango);
    Task EliminarAsync(int id);
}
