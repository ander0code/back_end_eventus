using back_end.Modules.reportes.DTOs;

namespace back_end.Modules.reportes.Repositories;

public interface IReporteRepository
{
    // Métricas - Clientes
    Task<IEnumerable<ClientesNuevosPorMesDto>> GetClientesNuevosPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<PromedioAdelantoPorClienteDto>> GetPromedioAdelantoPorClienteAsync(string? clienteId);
    Task<TasaRetencionClientesDto> GetTasaRetencionClientesAsync(DateTime? fechaInicio, DateTime? fechaFin);

    // Métricas - Inventario/Items
    Task<IEnumerable<ItemsMasUtilizadosDto>> GetItemsMasUtilizadosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10);
    Task<IEnumerable<StockPromedioPorTipoServicioDto>> GetStockPromedioPorTipoServicioAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<TasaDisponibilidadDto>> GetTasaDisponibilidadAsync();

    // Métricas - Pagos
    Task<IEnumerable<MontoPromedioPorPagoDto>> GetMontoPromedioPorPagoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<PromediotDiasReservaPagoDto> GetPromedioDiasReservaPagoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ReservasPagosIncompletosDto>> GetReservasPagosIncompletosAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<TasaUsoMetodoPagoDto>> GetTasaUsoMetodoPagoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<TendenciaMensualIngresosDto>> GetTendenciaMensualIngresosAsync(DateTime? fechaInicio, DateTime? fechaFin);

    // Métricas - Reservas
    Task<IEnumerable<ReservasPorMesDto>> GetReservasPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<IngresosPromedioPorTipoEventoDto>> GetIngresosPromedioPorTipoEventoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ReservasAdelantoAltoDto>> GetReservasAdelantoAltoAsync(decimal porcentajeMinimo = 50, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<DuracionPromedioReservasDto> GetDuracionPromedioReservasAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<TasaConversionEstadoDto> GetTasaConversionEstadoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<DistribucionReservasPorClienteDto>> GetDistribucionReservasPorClienteAsync(DateTime? fechaInicio, DateTime? fechaFin);

    // Métricas - Servicios
    Task<IEnumerable<ServiciosMasFrecuentesDto>> GetServiciosMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10);
    Task<IEnumerable<VariacionIngresosMensualesServicioDto>> GetVariacionIngresosMensualesServicioAsync(string? servicioId, DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<PromedioItemsPorServicioDto>> GetPromedioItemsPorServicioAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ServiciosSinReservasDto>> GetServiciosSinReservasAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ServiciosEventosCanceladosDto>> GetServiciosEventosCanceladosAsync(DateTime? fechaInicio, DateTime? fechaFin);

    // Resumen ejecutivo
    Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(DateTime? fechaInicio, DateTime? fechaFin);
}