using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioCalculadora
{
    Task<ResultadoCalculoDto> CalcularAsync(SolicitudCalculoDto solicitud);
}
