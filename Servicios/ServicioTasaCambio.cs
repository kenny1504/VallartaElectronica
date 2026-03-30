using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;

namespace ElectronicaVallarta.Servicios;

public class ServicioTasaCambio(
    IRepositorioTasaCambio repositorioTasaCambio,
    IRepositorioPais repositorioPais,
    IRepositorioSucursal repositorioSucursal) : IServicioTasaCambio
{
    public Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTasasAsync() => repositorioTasaCambio.ObtenerTodosAsync();
    public Task<TasaCambioRango?> ObtenerTasaPorIdAsync(int id, bool soloLectura = true) => repositorioTasaCambio.ObtenerPorIdAsync(id, soloLectura);

    public async Task CrearAsync(TasaCambioRango tasaCambioRango)
    {
        await ValidarModeloAsync(tasaCambioRango);
        tasaCambioRango.FechaCreacion = DateTime.UtcNow;
        await repositorioTasaCambio.AgregarAsync(tasaCambioRango);
    }

    public async Task ActualizarAsync(TasaCambioRango tasaCambioRango)
    {
        var tasaActual = await repositorioTasaCambio.ObtenerPorIdAsync(tasaCambioRango.Id, false)
                         ?? throw new InvalidOperationException("La tasa solicitada no existe.");

        await ValidarModeloAsync(tasaCambioRango, tasaCambioRango.Id);

        tasaActual.PaisId = tasaCambioRango.PaisId;
        tasaActual.SucursalId = tasaCambioRango.SucursalId;
        tasaActual.MontoDesdeUsd = tasaCambioRango.MontoDesdeUsd;
        tasaActual.MontoHastaUsd = tasaCambioRango.MontoHastaUsd;
        tasaActual.TasaCambio = tasaCambioRango.TasaCambio;
        tasaActual.FechaTasa = tasaCambioRango.FechaTasa.Date;
        tasaActual.EstaActivo = tasaCambioRango.EstaActivo;
        tasaActual.FechaActualizacion = DateTime.UtcNow;
        await repositorioTasaCambio.ActualizarAsync(tasaActual);
    }

    public async Task EliminarAsync(int id)
    {
        var tasa = await repositorioTasaCambio.ObtenerPorIdAsync(id, false)
                   ?? throw new InvalidOperationException("La tasa solicitada no existe.");

        await repositorioTasaCambio.EliminarAsync(tasa);
    }

    private async Task ValidarModeloAsync(TasaCambioRango tasaCambioRango, int? idExcluir = null)
    {
        if (tasaCambioRango.MontoDesdeUsd <= 0)
        {
            throw new InvalidOperationException("El monto desde debe ser mayor a cero.");
        }

        if (tasaCambioRango.MontoHastaUsd.HasValue && tasaCambioRango.MontoHastaUsd <= 0)
        {
            throw new InvalidOperationException("El monto hasta debe ser mayor a cero.");
        }

        if (tasaCambioRango.MontoHastaUsd.HasValue && tasaCambioRango.MontoDesdeUsd > tasaCambioRango.MontoHastaUsd)
        {
            throw new InvalidOperationException("El monto desde no puede ser mayor que el monto hasta.");
        }

        if (tasaCambioRango.TasaCambio <= 0)
        {
            throw new InvalidOperationException("La tasa de cambio debe ser mayor a cero.");
        }

        if (await repositorioPais.ObtenerPorIdAsync(tasaCambioRango.PaisId) is null)
        {
            throw new InvalidOperationException("El pais seleccionado no existe.");
        }

        var sucursal = await repositorioSucursal.ObtenerPorIdAsync(tasaCambioRango.SucursalId);
        if (sucursal is null || sucursal.PaisId != tasaCambioRango.PaisId)
        {
            throw new InvalidOperationException("La sucursal seleccionada no pertenece al pais indicado.");
        }

        tasaCambioRango.FechaTasa = tasaCambioRango.FechaTasa.Date;

        if (await repositorioTasaCambio.ExisteTraslapeAsync(tasaCambioRango, idExcluir))
        {
            throw new InvalidOperationException("Ya existe un rango traslapado para ese pais, sucursal y fecha.");
        }
    }
}
