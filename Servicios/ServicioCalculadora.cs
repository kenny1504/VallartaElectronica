using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;
using ElectronicaVallarta.Modelos.Dto;
using System.Globalization;

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
        var fechaOperacion = ObtenerFechaOperacion(solicitud.FechaCliente);
        var datosCalculo = await repositorioTasaCambio.ObtenerDatosCalculoAsync(
            solicitud.PaisId.Value,
            solicitud.SucursalId.Value,
            fechaOperacion,
            solicitud.MontoUsd.Value);

        if (datosCalculo is not null)
        {
            return new ResultadoCalculoDto
            {
                EsExitoso = true,
                Mensaje = "Calculo realizado correctamente.",
                TasaCambioRangoId = datosCalculo.TasaCambioRangoId,
                MontoUsd = solicitud.MontoUsd.Value,
                MontoRecibe = Math.Round(solicitud.MontoUsd.Value * datosCalculo.TasaCambio, 2, MidpointRounding.AwayFromZero),
                TasaCambioAplicada = datosCalculo.TasaCambio,
                RangoMontoDesdeUsd = datosCalculo.MontoDesdeUsd,
                RangoMontoHastaUsd = datosCalculo.MontoHastaUsd,
                DescripcionRangoAplicado = datosCalculo.MontoDesdeUsd.HasValue
                    ? $"{datosCalculo.MontoDesdeUsd.Value:N2} - {(datosCalculo.MontoHastaUsd.HasValue ? $"{datosCalculo.MontoHastaUsd.Value:N2}" : "En adelante")} USD"
                    : null,
                MonedaDestino = datosCalculo.CodigoMoneda,
                SimboloMoneda = datosCalculo.SimboloMoneda,
                NombrePais = datosCalculo.NombrePais,
                NombreSucursal = datosCalculo.NombreSucursal,
                FechaTasa = datosCalculo.FechaTasa
            };
        }

        var pais = await repositorioPais.ObtenerPorIdAsync(solicitud.PaisId.Value);
        if (pais is null || !pais.EstaActivo)
        {
            return new ResultadoCalculoDto
            {
                EsExitoso = false,
                Mensaje = "El pais seleccionado no esta disponible.",
                NombrePais = pais?.Nombre ?? string.Empty
            };
        }

        var sucursal = await repositorioSucursal.ObtenerPorIdAsync(solicitud.SucursalId.Value);
        if (sucursal is null || !sucursal.EstaActivo || sucursal.PaisId != solicitud.PaisId.Value)
        {
            return new ResultadoCalculoDto
            {
                EsExitoso = false,
                Mensaje = "La sucursal o canal seleccionado no corresponde al pais indicado.",
                NombrePais = pais.Nombre,
                NombreSucursal = sucursal?.Nombre ?? string.Empty
            };
        }

        return new ResultadoCalculoDto
        {
            EsExitoso = false,
            Mensaje = "No encontramos una tasa configurada para la fecha local del cliente con ese monto. Intenta con otro pagador o consulta con administracion.",
            NombrePais = pais.Nombre,
            NombreSucursal = sucursal.Nombre
        };
    }

    private static DateTime ObtenerFechaOperacion(string? fechaCliente)
    {
        if (DateTime.TryParseExact(fechaCliente, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
        {
            return fecha.Date;
        }

        return DateTime.Today;
    }
}
