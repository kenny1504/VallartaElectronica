using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioTasaCambio
{
    Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTodosAsync();
    Task<TasaCambioRango?> ObtenerPorIdAsync(int id, bool soloLectura = true);
    Task<TasaCambioRango?> ObtenerTasaAplicableAsync(int paisId, int sucursalId, DateTime fechaTasa, decimal montoUsd);
    Task<bool> ExisteTraslapeAsync(TasaCambioRango tasaCambioRango, int? idExcluir = null);
    Task AgregarAsync(TasaCambioRango tasaCambioRango);
    Task ActualizarAsync(TasaCambioRango tasaCambioRango);
    Task EliminarAsync(TasaCambioRango tasaCambioRango);
}
