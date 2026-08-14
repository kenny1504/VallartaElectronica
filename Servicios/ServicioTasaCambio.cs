using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Servicios;

public class ServicioTasaCambio(
    IRepositorioTasaCambio repositorioTasaCambio,
    IRepositorioPais repositorioPais,
    IRepositorioSucursal repositorioSucursal) : IServicioTasaCambio
{
    public Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTasasAsync(DateTime? fechaFiltro = null) => repositorioTasaCambio.ObtenerTodosAsync(fechaFiltro);
    public Task<IReadOnlyCollection<RegistroTasaCambioListadoDto>> ObtenerListadoTasasAsync(DateTime? fechaFiltro = null, int? paisIdFiltro = null) => repositorioTasaCambio.ObtenerListadoAsync(fechaFiltro, paisIdFiltro);
    public Task<TasaCambioRango?> ObtenerTasaPorIdAsync(int id, bool soloLectura = true) => repositorioTasaCambio.ObtenerPorIdAsync(id, soloLectura);

    public async Task CrearAsync(TasaCambioRango tasaCambioRango)
    {
        await ValidarModeloAsync(tasaCambioRango);
        tasaCambioRango.FechaCreacion = DateTime.UtcNow;
        await repositorioTasaCambio.AgregarAsync(tasaCambioRango);
    }

    public async Task<int> CopiarAsync(DateTime? fechaOrigen, DateTime fechaDestino, bool copiarTodas, IReadOnlyCollection<int> tasasSeleccionadas, int? paisIdFiltro = null)
    {
        var tasasOrigen = await ObtenerTasasOrigenAsync(fechaOrigen, copiarTodas, tasasSeleccionadas, paisIdFiltro);
        if (tasasOrigen.Count == 0)
        {
            throw new InvalidOperationException("No se encontraron tasas para copiar.");
        }

        var fechaDestinoNormalizada = fechaDestino.Date;
        var fechaCreacion = DateTime.UtcNow;
        var tasasCopiadas = tasasOrigen.Select(x => new TasaCambioRango
        {
            PaisId = x.PaisId,
            SucursalId = x.SucursalId,
            MontoDesdeUsd = x.MontoDesdeUsd,
            MontoHastaUsd = x.MontoHastaUsd,
            TasaCambio = x.TasaCambio,
            FechaTasa = fechaDestinoNormalizada,
            EstaActivo = x.EstaActivo,
            FechaCreacion = fechaCreacion
        }).ToList();

        ValidarTraslapesEnLote(tasasCopiadas);

        foreach (var tasa in tasasCopiadas)
        {
            var tasaExistente = await repositorioTasaCambio.ObtenerPorRangoAsync(tasa.PaisId, tasa.SucursalId, tasa.FechaTasa, tasa.MontoDesdeUsd, tasa.MontoHastaUsd);
            await ValidarModeloAsync(tasa, tasaExistente?.Id);
        }

        await repositorioTasaCambio.GuardarCopiaAsync(tasasCopiadas);
        return tasasCopiadas.Count;
    }

    public async Task<int> ActualizarTasasEnLoteAsync(DateTime fechaTasa, int paisId, IReadOnlyCollection<ActualizacionTasaCambioMasivaDto> actualizaciones)
    {
        var actualizacionesValidas = actualizaciones
            .Where(x => x.Id > 0)
            .ToList();

        if (actualizacionesValidas.Count == 0)
        {
            throw new InvalidOperationException("No hay tasas para actualizar.");
        }

        if (actualizacionesValidas.Select(x => x.Id).Distinct().Count() != actualizacionesValidas.Count)
        {
            throw new InvalidOperationException("La solicitud contiene tasas repetidas.");
        }

        if (actualizacionesValidas.Any(x => x.TasaCambio <= 0))
        {
            throw new InvalidOperationException("Todas las tasas deben ser mayores a cero.");
        }

        return await repositorioTasaCambio.ActualizarValoresEnLoteAsync(fechaTasa.Date, paisId, actualizacionesValidas);
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

    private async Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTasasOrigenAsync(DateTime? fechaOrigen, bool copiarTodas, IReadOnlyCollection<int> tasasSeleccionadas, int? paisIdFiltro)
    {
        if (copiarTodas)
        {
            if (!fechaOrigen.HasValue)
            {
                throw new InvalidOperationException("Selecciona una fecha origen para copiar todas las tasas.");
            }

            var tasasPorFecha = await repositorioTasaCambio.ObtenerTodosAsync(fechaOrigen.Value.Date);
            return paisIdFiltro.HasValue
                ? tasasPorFecha.Where(x => x.PaisId == paisIdFiltro.Value).ToList()
                : tasasPorFecha;
        }

        var ids = tasasSeleccionadas.Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
        {
            throw new InvalidOperationException("Selecciona al menos una tasa para copiar.");
        }

        return await repositorioTasaCambio.ObtenerPorIdsAsync(ids);
    }

    private static void ValidarTraslapesEnLote(IReadOnlyCollection<TasaCambioRango> tasasCambioRango)
    {
        foreach (var grupo in tasasCambioRango.GroupBy(x => new { x.PaisId, x.SucursalId, FechaTasa = x.FechaTasa.Date }))
        {
            var tasas = grupo.OrderBy(x => x.MontoDesdeUsd).ToList();
            for (var indice = 1; indice < tasas.Count; indice++)
            {
                var tasaAnterior = tasas[indice - 1];
                var tasaActual = tasas[indice];
                if (tasaActual.MontoDesdeUsd <= (tasaAnterior.MontoHastaUsd ?? decimal.MaxValue))
                {
                    throw new InvalidOperationException("La seleccion contiene rangos traslapados para el mismo pais, sucursal y fecha destino.");
                }
            }
        }
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

        if (!await repositorioPais.ExisteActivoAsync(tasaCambioRango.PaisId))
        {
            throw new InvalidOperationException("El pais seleccionado no existe.");
        }

        if (!await repositorioSucursal.ExisteActivaEnPaisAsync(tasaCambioRango.SucursalId, tasaCambioRango.PaisId))
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
