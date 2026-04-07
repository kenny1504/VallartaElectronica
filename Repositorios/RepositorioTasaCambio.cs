using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Modelos.Dto;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Repositorios;

public class RepositorioTasaCambio(ContextoAplicacion contexto) : IRepositorioTasaCambio
{
    public async Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTodosAsync(DateTime? fechaFiltro = null)
    {
        var consulta = contexto.TasasCambioRango.AsNoTracking()
            .Include(x => x.Pais)
            .Include(x => x.Sucursal)
            .AsQueryable();

        if (fechaFiltro.HasValue)
        {
            var fecha = fechaFiltro.Value.Date;
            consulta = consulta.Where(x => x.FechaTasa == fecha);
        }

        return await consulta
            .OrderByDescending(x => x.FechaTasa)
            .ThenBy(x => x.Pais!.Nombre)
            .ThenBy(x => x.Sucursal!.Nombre)
            .ThenBy(x => x.MontoDesdeUsd)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<RegistroTasaCambioListadoDto>> ObtenerListadoAsync(DateTime? fechaFiltro = null, int? paisIdFiltro = null)
    {
        var consulta = contexto.TasasCambioRango.AsNoTracking().AsQueryable();

        if (fechaFiltro.HasValue)
        {
            var fecha = fechaFiltro.Value.Date;
            consulta = consulta.Where(x => x.FechaTasa == fecha);
        }

        if (paisIdFiltro.HasValue)
        {
            consulta = consulta.Where(x => x.PaisId == paisIdFiltro.Value);
        }

        // Proyecta solo columnas visibles en la tabla para evitar Includes y entidades completas.
        return await consulta
            .OrderByDescending(x => x.FechaTasa)
            .ThenBy(x => x.Pais!.Nombre)
            .ThenBy(x => x.Sucursal!.Nombre)
            .ThenBy(x => x.MontoDesdeUsd)
            .Select(x => new RegistroTasaCambioListadoDto
            {
                Id = x.Id,
                FechaTasa = x.FechaTasa,
                NombrePais = x.Pais!.Nombre,
                NombreSucursal = x.Sucursal!.Nombre,
                MontoDesdeUsd = x.MontoDesdeUsd,
                MontoHastaUsd = x.MontoHastaUsd,
                TasaCambio = x.TasaCambio,
                EstaActivo = x.EstaActivo
            })
            .ToListAsync();
    }

    public async Task<TasaCambioRango?> ObtenerPorIdAsync(int id, bool soloLectura = true)
    {
        IQueryable<TasaCambioRango> consulta = contexto.TasasCambioRango;
        if (soloLectura)
        {
            consulta = consulta.AsNoTracking().Include(x => x.Pais).Include(x => x.Sucursal);
        }
        else
        {
            consulta = consulta.AsTracking();
        }

        return await consulta.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TasaCambioRango?> ObtenerTasaAplicableAsync(int paisId, int sucursalId, DateTime fechaTasa, decimal montoUsd)
    {
        var fecha = fechaTasa.Date;
        return await contexto.TasasCambioRango.AsNoTracking()
            .Where(x => x.EstaActivo &&
                        x.PaisId == paisId &&
                        x.SucursalId == sucursalId &&
                        x.FechaTasa == fecha &&
                        montoUsd >= x.MontoDesdeUsd &&
                        (!x.MontoHastaUsd.HasValue || montoUsd <= x.MontoHastaUsd.Value))
            .OrderByDescending(x => x.MontoDesdeUsd)
            .FirstOrDefaultAsync();
    }

    public Task<DatosCalculoCotizacionDto?> ObtenerDatosCalculoAsync(int paisId, int sucursalId, DateTime fechaTasa, decimal montoUsd)
    {
        var fecha = fechaTasa.Date;

        // Resuelve el camino exitoso de la cotización en una sola consulta SQL.
        return contexto.TasasCambioRango.AsNoTracking()
            .Where(x => x.EstaActivo &&
                        x.PaisId == paisId &&
                        x.SucursalId == sucursalId &&
                        x.FechaTasa == fecha &&
                        montoUsd >= x.MontoDesdeUsd &&
                        (!x.MontoHastaUsd.HasValue || montoUsd <= x.MontoHastaUsd.Value) &&
                        x.Pais != null &&
                        x.Pais.EstaActivo &&
                        x.Sucursal != null &&
                        x.Sucursal.EstaActivo)
            .OrderByDescending(x => x.MontoDesdeUsd)
            .Select(x => new DatosCalculoCotizacionDto
            {
                TasaCambioRangoId = x.Id,
                TasaCambio = x.TasaCambio,
                FechaTasa = x.FechaTasa,
                MontoDesdeUsd = x.MontoDesdeUsd,
                MontoHastaUsd = x.MontoHastaUsd,
                CodigoMoneda = x.Pais!.CodigoMoneda,
                SimboloMoneda = x.Pais!.SimboloMoneda,
                NombrePais = x.Pais!.Nombre,
                NombreSucursal = x.Sucursal!.Nombre
            })
            .FirstOrDefaultAsync();
    }

    public Task<bool> ExisteTraslapeAsync(TasaCambioRango tasaCambioRango, int? idExcluir = null)
    {
        var montoHastaActual = tasaCambioRango.MontoHastaUsd ?? decimal.MaxValue;

        // Evalúa el traslape en SQL para no cargar todos los rangos a memoria.
        return contexto.TasasCambioRango.AsNoTracking()
            .Where(x => x.PaisId == tasaCambioRango.PaisId &&
                        x.SucursalId == tasaCambioRango.SucursalId &&
                        x.FechaTasa == tasaCambioRango.FechaTasa.Date &&
                        (!idExcluir.HasValue || x.Id != idExcluir.Value))
            .AnyAsync(x => tasaCambioRango.MontoDesdeUsd <= (x.MontoHastaUsd ?? decimal.MaxValue) &&
                           x.MontoDesdeUsd <= montoHastaActual);
    }

    public async Task AgregarAsync(TasaCambioRango tasaCambioRango)
    {
        await contexto.TasasCambioRango.AddAsync(tasaCambioRango);
        await contexto.SaveChangesAsync();
    }

    public async Task ActualizarAsync(TasaCambioRango tasaCambioRango)
    {
        contexto.TasasCambioRango.Update(tasaCambioRango);
        await contexto.SaveChangesAsync();
    }

    public async Task EliminarAsync(TasaCambioRango tasaCambioRango)
    {
        contexto.TasasCambioRango.Remove(tasaCambioRango);
        await contexto.SaveChangesAsync();
    }
}
