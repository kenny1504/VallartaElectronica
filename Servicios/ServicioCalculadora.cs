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

        // El caso exitoso se resuelve con una sola consulta proyectada desde tasas.
        var datosCalculo = await repositorioTasaCambio.ObtenerDatosCalculoAsync(
            solicitud.PaisId.Value,
            solicitud.SucursalId.Value,
            DateTime.Today,
            solicitud.MontoUsd.Value);

        if (datosCalculo is not null)
        {
            return new ResultadoCalculoDto
            {
                EsExitoso = true,
                Mensaje = "Calculo realizado correctamente.",
                MontoUsd = solicitud.MontoUsd.Value,
                MontoRecibe = Math.Round(solicitud.MontoUsd.Value * datosCalculo.TasaCambio, 2, MidpointRounding.AwayFromZero),
                TasaCambioAplicada = datosCalculo.TasaCambio,
                MonedaDestino = datosCalculo.CodigoMoneda,
                SimboloMoneda = datosCalculo.SimboloMoneda,
                NombrePais = datosCalculo.NombrePais,
                NombreSucursal = datosCalculo.NombreSucursal,
                FechaTasa = datosCalculo.FechaTasa
            };
        }

        if (!await repositorioPais.ExisteActivoAsync(solicitud.PaisId.Value))
        {
            return new ResultadoCalculoDto { EsExitoso = false, Mensaje = "El pais seleccionado no esta disponible." };
        }

        if (!await repositorioSucursal.ExisteActivaEnPaisAsync(solicitud.SucursalId.Value, solicitud.PaisId.Value))
        {
            return new ResultadoCalculoDto { EsExitoso = false, Mensaje = "La sucursal o canal seleccionado no corresponde al pais indicado." };
        }

        return new ResultadoCalculoDto { EsExitoso = false, Mensaje = "No encontramos una tasa configurada para hoy con ese monto. Intenta con otro canal o consulta con administracion." };
    }
}
