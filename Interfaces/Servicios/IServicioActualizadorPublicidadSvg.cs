namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioActualizadorPublicidadSvg
{
    Task<ResultadoActualizacionPublicidadSvg> ActualizarAsync(DateTime fechaTasa);
}

public sealed record ResultadoActualizacionPublicidadSvg(bool Success, string Message);
