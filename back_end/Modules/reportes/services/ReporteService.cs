using back_end.Modules.reportes.DTOs;
using back_end.Modules.reportes.Repositories;

namespace back_end.Modules.reportes.Services;

public class ReporteService : IReporteService
{
    private readonly IReporteRepository _reporteRepository;

    public ReporteService(IReporteRepository reporteRepository)
    {
        _reporteRepository = reporteRepository;
    }

    public async Task<IEnumerable<ReporteItemDto>> GetReporteItemsAsync(ReporteItemParametrosDto parametros)
    {
        return await _reporteRepository.GetReporteItemsAsync(parametros);
    }

    public async Task<IEnumerable<ReportePagoDto>> GetReportePagosAsync(ReportePagoParametrosDto parametros)
    {
        return await _reporteRepository.GetReportePagosAsync(parametros);
    }

    public async Task<IEnumerable<ReporteClienteDto>> GetReporteClientesAsync(ReporteClienteParametrosDto parametros)
    {
        return await _reporteRepository.GetReporteClientesAsync(parametros);
    }

    public async Task<IEnumerable<ReporteReservaDto>> GetReporteReservasAsync(ReporteReservaParametrosDto parametros)
    {
        return await _reporteRepository.GetReporteReservasAsync(parametros);
    }

    public async Task<IEnumerable<ReporteServicioDto>> GetReporteServiciosAsync(ReporteServicioParametrosDto parametros)
    {
        return await _reporteRepository.GetReporteServiciosAsync(parametros);
    }
}