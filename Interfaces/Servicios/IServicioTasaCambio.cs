using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioTasaCambio
{
    Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTasasAsync(DateTime? fechaFiltro = null);
    Task<IReadOnlyCollection<RegistroTasaCambioListadoDto>> ObtenerListadoTasasAsync(DateTime? fechaFiltro = null);
    Task<TasaCambioRango?> ObtenerTasaPorIdAsync(int id, bool soloLectura = true);
    Task CrearAsync(TasaCambioRango tasaCambioRango);
    Task ActualizarAsync(TasaCambioRango tasaCambioRango);
    Task EliminarAsync(int id);
}
