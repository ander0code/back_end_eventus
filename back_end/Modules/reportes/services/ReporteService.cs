using back_end.Modules.reportes.DTOs;
using back_end.Modules.reportes.Repositories;


namespace back_end.Modules.reportes.Services;

public class ReporteService : IReporteService
{
    private readonly IClientesReporteRepository _clientesReporteRepository;
    private readonly IInventarioReporteRepository _inventarioReporteRepository;
    private readonly IPagosReporteRepository _pagosReporteRepository;
    private readonly IReservasReporteRepository _reservasReporteRepository;
    private readonly IServiciosReporteRepository _serviciosReporteRepository;
    private readonly IResumenEjecutivoRepository _resumenEjecutivoRepository;

    public ReporteService(
        IClientesReporteRepository clientesReporteRepository,
        IInventarioReporteRepository inventarioReporteRepository,
        IPagosReporteRepository pagosReporteRepository,
        IReservasReporteRepository reservasReporteRepository,
        IServiciosReporteRepository serviciosReporteRepository,
        IResumenEjecutivoRepository resumenEjecutivoRepository)
    {
        _clientesReporteRepository = clientesReporteRepository;
        _inventarioReporteRepository = inventarioReporteRepository;
        _pagosReporteRepository = pagosReporteRepository;
        _reservasReporteRepository = reservasReporteRepository;
        _serviciosReporteRepository = serviciosReporteRepository;
        _resumenEjecutivoRepository = resumenEjecutivoRepository;
    }

    // Métricas - CLIENTES
    public async Task<IEnumerable<ClientesNuevosPorMesDto>> GetClientesNuevosPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _clientesReporteRepository.GetClientesNuevosPorMesAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<PromedioAdelantoPorClienteDto>> GetPromedioAdelantoPorClienteAsync(string? clienteId)
    {
        return await _clientesReporteRepository.GetPromedioAdelantoPorClienteAsync(clienteId);
    }

    public async Task<TasaRetencionClientesDto> GetTasaRetencionClientesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _clientesReporteRepository.GetTasaRetencionClientesAsync(fechaInicio, fechaFin);
    }

    // Métricas - INVENTARIO/ITEMS
    public async Task<IEnumerable<ItemsMasUtilizadosDto>> GetItemsMasUtilizadosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        return await _inventarioReporteRepository.GetItemsMasUtilizadosAsync(fechaInicio, fechaFin, top);
    }

    public async Task<IEnumerable<StockPromedioPorTipoServicioDto>> GetStockPromedioPorTipoServicioAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _inventarioReporteRepository.GetStockPromedioPorTipoServicioAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<TasaDisponibilidadDto>> GetTasaDisponibilidadAsync()
    {
        return await _inventarioReporteRepository.GetTasaDisponibilidadAsync();
    }

    // Métricas - PAGOS
    public async Task<IEnumerable<MontoPromedioPorPagoDto>> GetMontoPromedioPorPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _pagosReporteRepository.GetMontoPromedioPorPagoAsync(fechaInicio, fechaFin);
    }

    public async Task<PromediotDiasReservaPagoDto> GetPromedioDiasReservaPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _pagosReporteRepository.GetPromedioDiasReservaPagoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ReservasPagosIncompletosDto>> GetReservasPagosIncompletosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _pagosReporteRepository.GetReservasPagosIncompletosAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<TasaUsoMetodoPagoDto>> GetTasaUsoMetodoPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _pagosReporteRepository.GetTasaUsoMetodoPagoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<TendenciaMensualIngresosDto>> GetTendenciaMensualIngresosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _pagosReporteRepository.GetTendenciaMensualIngresosAsync(fechaInicio, fechaFin);
    }

    // Métricas - RESERVAS
    public async Task<IEnumerable<ReservasPorMesDto>> GetReservasPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reservasReporteRepository.GetReservasPorMesAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<IngresosPromedioPorTipoEventoDto>> GetIngresosPromedioPorTipoEventoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reservasReporteRepository.GetIngresosPromedioPorTipoEventoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ReservasAdelantoAltoDto>> GetReservasAdelantoAltoAsync(decimal porcentajeMinimo = 50, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        return await _reservasReporteRepository.GetReservasAdelantoAltoAsync(porcentajeMinimo, fechaInicio, fechaFin);
    }

    public async Task<DuracionPromedioReservasDto> GetDuracionPromedioReservasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reservasReporteRepository.GetDuracionPromedioReservasAsync(fechaInicio, fechaFin);
    }

    public async Task<TasaConversionEstadoDto> GetTasaConversionEstadoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _reservasReporteRepository.GetTasaConversionEstadoAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<DistribucionReservasPorClienteDto>> GetDistribucionReservasPorClienteAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _clientesReporteRepository.GetDistribucionReservasPorClienteAsync(fechaInicio, fechaFin);
    }

    // Métricas - SERVICIOS
    public async Task<IEnumerable<ServiciosMasFrecuentesDto>> GetServiciosMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        return await _serviciosReporteRepository.GetServiciosMasFrecuentesAsync(fechaInicio, fechaFin, top);
    }

    public async Task<IEnumerable<VariacionIngresosMensualesServicioDto>> GetVariacionIngresosMensualesServicioAsync(string? servicioId, DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _serviciosReporteRepository.GetVariacionIngresosMensualesServicioAsync(servicioId, fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<PromedioItemsPorServicioDto>> GetPromedioItemsPorServicioAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _serviciosReporteRepository.GetPromedioItemsPorServicioAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ServiciosSinReservasDto>> GetServiciosSinReservasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _serviciosReporteRepository.GetServiciosSinReservasAsync(fechaInicio, fechaFin);
    }

    public async Task<IEnumerable<ServiciosEventosCanceladosDto>> GetServiciosEventosCanceladosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _serviciosReporteRepository.GetServiciosEventosCanceladosAsync(fechaInicio, fechaFin);
    }

    // RESUMEN EJECUTIVO
    public async Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        return await _resumenEjecutivoRepository.GetResumenEjecutivoAsync(fechaInicio, fechaFin);
    }
}