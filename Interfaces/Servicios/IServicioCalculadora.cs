using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioCalculadora
{
    /// <summary>
    /// Realiza un cálculo basado en los datos proporcionados en la solicitud y devuelve los resultados
    /// correspondientes.
    /// </summary>
    /// <param name="solicitud">
    /// Objeto de tipo <see cref="SolicitudCalculoDto"/> que contiene los datos necesarios para llevar
    /// a cabo el cálculo, como el país, la sucursal y el monto en USD.
    /// </param>
    /// <returns>
    /// Un objeto de tipo <see cref="ResultadoCalculoDto"/> que incluye los resultados del cálculo, como
    /// el monto convertido, la tasa de cambio aplicada, y detalles adicionales sobre el rango aplicado
    /// y la moneda de destino.
    /// </returns>
    Task<ResultadoCalculoDto> CalcularAsync(SolicitudCalculoDto solicitud);
}
