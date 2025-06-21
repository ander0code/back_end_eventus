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

    // Métricas - CLIENTES
    public async Task<IEnumerable<ClientesNuevosPorMesDto>> GetClientesNuevosPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetClientesNuevosPorMesAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<PromedioAdelantoPorClienteDto>> GetPromedioAdelantoPorClienteAsync(string? clienteId)
    {
        return await _reporteRepository.GetPromedioAdelantoPorClienteAsync(clienteId);
    }

    public async Task<TasaRetencionClientesDto> GetTasaRetencionClientesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetTasaRetencionClientesAsync(fechaInicio, fechaFin);
    }

    // Métricas - INVENTARIO/ITEMS
    public async Task<IEnumerable<ItemsMasUtilizadosDto>> GetItemsMasUtilizadosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        return await _reporteRepository.GetItemsMasUtilizadosAsync(fechaInicio, fechaFin, top);
    }

    public async Task<IEnumerable<StockPromedioPorTipoServicioDto>> GetStockPromedioPorTipoServicioAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetStockPromedioPorTipoServicioAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<TasaDisponibilidadDto>> GetTasaDisponibilidadAsync()
    {
        return await _reporteRepository.GetTasaDisponibilidadAsync();
    }

    // Métricas - PAGOS
    public async Task<IEnumerable<MontoPromedioPorPagoDto>> GetMontoPromedioPorPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetMontoPromedioPorPagoAsync(fechaInicio, fechaFin);
    }

    public async Task<PromediotDiasReservaPagoDto> GetPromedioDiasReservaPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetPromedioDiasReservaPagoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ReservasPagosIncompletosDto>> GetReservasPagosIncompletosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetReservasPagosIncompletosAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<TasaUsoMetodoPagoDto>> GetTasaUsoMetodoPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetTasaUsoMetodoPagoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<TendenciaMensualIngresosDto>> GetTendenciaMensualIngresosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetTendenciaMensualIngresosAsync(fechaInicio, fechaFin);
    }

    // Métricas - RESERVAS
    public async Task<IEnumerable<ReservasPorMesDto>> GetReservasPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetReservasPorMesAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<IngresosPromedioPorTipoEventoDto>> GetIngresosPromedioPorTipoEventoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetIngresosPromedioPorTipoEventoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ReservasAdelantoAltoDto>> GetReservasAdelantoAltoAsync(decimal porcentajeMinimo = 50, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        return await _reporteRepository.GetReservasAdelantoAltoAsync(porcentajeMinimo, fechaInicio, fechaFin);
    }

    public async Task<DuracionPromedioReservasDto> GetDuracionPromedioReservasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetDuracionPromedioReservasAsync(fechaInicio, fechaFin);
    }

    public async Task<TasaConversionEstadoDto> GetTasaConversionEstadoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetTasaConversionEstadoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<DistribucionReservasPorClienteDto>> GetDistribucionReservasPorClienteAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetDistribucionReservasPorClienteAsync(fechaInicio, fechaFin);
    }

    // Métricas - SERVICIOS
    public async Task<IEnumerable<ServiciosMasFrecuentesDto>> GetServiciosMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        return await _reporteRepository.GetServiciosMasFrecuentesAsync(fechaInicio, fechaFin, top);
    }

    public async Task<IEnumerable<VariacionIngresosMensualesServicioDto>> GetVariacionIngresosMensualesServicioAsync(Guid? servicioId, DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetVariacionIngresosMensualesServicioAsync(servicioId, fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<PromedioItemsPorServicioDto>> GetPromedioItemsPorServicioAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetPromedioItemsPorServicioAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ServiciosSinReservasDto>> GetServiciosSinReservasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetServiciosSinReservasAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ServiciosEventosCanceladosDto>> GetServiciosEventosCanceladosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetServiciosEventosCanceladosAsync(fechaInicio, fechaFin);
    }

    // RESUMEN EJECUTIVO
    public async Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reporteRepository.GetResumenEjecutivoAsync(fechaInicio, fechaFin);
    }
}