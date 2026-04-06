using ElectronicaVallarta.Modelos.Dto;
using ElectronicaVallarta.ViewModels;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioAnaliticaConsultas
{
    /// Registra una consulta analítica en el sistema de manera asincrónica.
    /// <param name="registroConsulta">
    /// Un objeto de tipo `RegistroConsultaAnaliticaEntradaDto` que contiene los datos relacionados con la consulta realizada,
    /// como fecha, sucursal, país, rango de monto, tasa de cambio aplicada, entre otros.
    /// </param>
    /// <return>
    /// Una tarea que representa la operación asincrónica de registro de la consulta.
    /// </return>
    Task RegistrarConsultaAsync(RegistroConsultaAnaliticaEntradaDto registroConsulta);

    /// Obtiene un panel de análisis de consultas que incluye información de resumen,
    /// opciones de filtro y un listado detallado de datos paginados, basado en los criterios especificados por un filtro.
    /// <param name="filtro">
    /// Objeto que contiene los criterios para filtrar los datos analíticos. Puede incluir propiedades como rango de fechas,
    /// país, región, sucursal, entre otros parámetros de consulta.
    /// </param>
    /// <return>
    /// Una instancia de <see cref="PanelAnaliticaConsultasViewModel"/> que contiene el resumen de datos,
    /// listado paginado y opciones de filtro relacionadas con el análisis.
    /// </return>
    Task<PanelAnaliticaConsultasViewModel> ObtenerPanelAsync(FiltroConsultaAnaliticaDto filtro);

    /// Exporta las consultas analíticas basadas en los criterios de filtro proporcionados, generando un archivo en formato Excel.
    /// <param name="filtro">
    /// Objeto de tipo FiltroConsultaAnaliticaDto que contiene los criterios para filtrar las consultas.
    /// Los criterios incluyen fechas, ubicación, rango, resultados, entre otros.
    /// </param>
    /// <return>
    /// Un objeto de tipo ArchivoExportacionDto que contiene los datos exportados, incluyendo el nombre del archivo,
    /// el tipo de contenido del archivo, y los datos binarios del mismo.
    /// </return>
    Task<ArchivoExportacionDto> ExportarAsync(FiltroConsultaAnaliticaDto filtro);
}
