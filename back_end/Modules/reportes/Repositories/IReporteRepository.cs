using back_end.Modules.reportes.DTOs;

namespace back_end.Modules.reportes.Repositories;

public interface IReporteRepository
{
    Task<IEnumerable<ReporteItemDto>> GetReporteItemsAsync(ReporteItemParametrosDto parametros);
    Task<IEnumerable<ReportePagoDto>> GetReportePagosAsync(ReportePagoParametrosDto parametros);
    Task<IEnumerable<ReporteClienteDto>> GetReporteClientesAsync(ReporteClienteParametrosDto parametros);
    Task<IEnumerable<ReporteReservaDto>> GetReporteReservasAsync(ReporteReservaParametrosDto parametros);
    Task<IEnumerable<ReporteServicioDto>> GetReporteServiciosAsync(ReporteServicioParametrosDto parametros);
}