namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioActualizadorPublicidadSvg
{
    Task<ResultadoActualizacionPublicidadSvg> ActualizarAsync();
}

public sealed record ResultadoActualizacionPublicidadSvg(bool Success, string Message);
