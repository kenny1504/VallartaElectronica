using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Servicios;

public class ServicioCalculadora(
    IRepositorioPais repositorioPais,
    IRepositorioSucursal repositorioSucursal,
    IRepositorioTasaCambio repositorioTasaCambio) : IServicioCalculadora
{
    public async Task<ResultadoCalculoDto> CalcularAsync(SolicitudCalculoDto solicitud)
    {
        if (!solicitud.PaisId.HasValue || !solicitud.SucursalId.HasValue || !solicitud.MontoUsd.HasValue || solicitud.MontoUsd <= 0)
        {
            return new ResultadoCalculoDto { EsExitoso = false, Mensaje = "Completa todos los datos para realizar el calculo." };
        }

        var pais = await repositorioPais.ObtenerPorIdAsync(solicitud.PaisId.Value);
        if (pais is null || !pais.EstaActivo)
        {
            return new ResultadoCalculoDto { EsExitoso = false, Mensaje = "El pais seleccionado no esta disponible." };
        }

        var sucursal = await repositorioSucursal.ObtenerPorIdAsync(solicitud.SucursalId.Value);
        if (sucursal is null || !sucursal.EstaActivo || sucursal.PaisId != pais.Id)
        {
            return new ResultadoCalculoDto { EsExitoso = false, Mensaje = "La sucursal o canal seleccionado no corresponde al pais indicado." };
        }

        var tasaAplicable = await repositorioTasaCambio.ObtenerTasaAplicableAsync(pais.Id, sucursal.Id, DateTime.Today, solicitud.MontoUsd.Value);
        if (tasaAplicable is null)
        {
            return new ResultadoCalculoDto { EsExitoso = false, Mensaje = "No encontramos una tasa configurada para hoy con ese monto. Intenta con otro canal o consulta con administracion." };
        }

        return new ResultadoCalculoDto
        {
            EsExitoso = true,
            Mensaje = "Calculo realizado correctamente.",
            MontoUsd = solicitud.MontoUsd.Value,
            MontoRecibe = Math.Round(solicitud.MontoUsd.Value * tasaAplicable.TasaCambio, 2, MidpointRounding.AwayFromZero),
            TasaCambioAplicada = tasaAplicable.TasaCambio,
            MonedaDestino = pais.CodigoMoneda,
            SimboloMoneda = pais.SimboloMoneda,
            NombrePais = pais.Nombre,
            NombreSucursal = sucursal.Nombre,
            FechaTasa = tasaAplicable.FechaTasa
        };
    }
}
