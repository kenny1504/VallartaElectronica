using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
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

    public async Task<TasaCambioRango?> ObtenerPorIdAsync(int id, bool soloLectura = true)
    {
        var consulta = contexto.TasasCambioRango.Include(x => x.Pais).Include(x => x.Sucursal).AsQueryable();
        if (soloLectura)
        {
            consulta = consulta.AsNoTracking();
        }

        return await consulta.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TasaCambioRango?> ObtenerTasaAplicableAsync(int paisId, int sucursalId, DateTime fechaTasa, decimal montoUsd)
    {
        var fecha = fechaTasa.Date;
        return await contexto.TasasCambioRango.AsNoTracking()
            .Include(x => x.Pais)
            .Include(x => x.Sucursal)
            .Where(x => x.EstaActivo &&
                        x.PaisId == paisId &&
                        x.SucursalId == sucursalId &&
                        x.FechaTasa == fecha &&
                        montoUsd >= x.MontoDesdeUsd &&
                        (!x.MontoHastaUsd.HasValue || montoUsd <= x.MontoHastaUsd.Value))
            .OrderByDescending(x => x.MontoDesdeUsd)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExisteTraslapeAsync(TasaCambioRango tasaCambioRango, int? idExcluir = null)
    {
        var montoHastaActual = tasaCambioRango.MontoHastaUsd ?? decimal.MaxValue;
        var tasasExistentes = await contexto.TasasCambioRango.AsNoTracking()
            .Where(x => x.PaisId == tasaCambioRango.PaisId &&
                        x.SucursalId == tasaCambioRango.SucursalId &&
                        x.FechaTasa == tasaCambioRango.FechaTasa.Date &&
                        (!idExcluir.HasValue || x.Id != idExcluir.Value))
            .ToListAsync();

        return tasasExistentes.Any(x =>
        {
            var montoHastaExistente = x.MontoHastaUsd ?? decimal.MaxValue;
            return tasaCambioRango.MontoDesdeUsd <= montoHastaExistente &&
                   x.MontoDesdeUsd <= montoHastaActual;
        });
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
