using Microsoft.AspNetCore.Mvc;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.reportes.Services;

namespace back_end.Modules.reportes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    // MÉTRICAS - CLIENTES
    [HttpGet("clientes/nuevos-por-mes")]
    public async Task<ActionResult<IEnumerable<ClientesNuevosPorMesDto>>> GetClientesNuevosPorMes(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetClientesNuevosPorMesAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("clientes/promedio-adelanto")]
    public async Task<ActionResult<IEnumerable<PromedioAdelantoPorClienteDto>>> GetPromedioAdelantoPorCliente(
        [FromQuery] string? clienteId)
    {
        try
        {
            var resultado = await _reporteService.GetPromedioAdelantoPorClienteAsync(clienteId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("clientes/tasa-retencion")]
    public async Task<ActionResult<TasaRetencionClientesDto>> GetTasaRetencionClientes(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetTasaRetencionClientesAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // MÉTRICAS - INVENTARIO/ITEMS
    [HttpGet("items/mas-utilizados")]
    public async Task<ActionResult<IEnumerable<ItemsMasUtilizadosDto>>> GetItemsMasUtilizados(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] int top = 10)
    {
        try
        {
            var resultado = await _reporteService.GetItemsMasUtilizadosAsync(fechaInicio, fechaFin, top);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("items/stock-promedio-por-servicio")]
    public async Task<ActionResult<IEnumerable<StockPromedioPorTipoServicioDto>>> GetStockPromedioPorTipoServicio(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetStockPromedioPorTipoServicioAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("items/tasa-disponibilidad")]
    public async Task<ActionResult<IEnumerable<TasaDisponibilidadDto>>> GetTasaDisponibilidad()
    {
        try
        {
            var resultado = await _reporteService.GetTasaDisponibilidadAsync();
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // MÉTRICAS - PAGOS
    [HttpGet("pagos/monto-promedio")]
    public async Task<ActionResult<IEnumerable<MontoPromedioPorPagoDto>>> GetMontoPromedioPorPago(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetMontoPromedioPorPagoAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("pagos/promedio-dias-reserva-pago")]
    public async Task<ActionResult<PromediotDiasReservaPagoDto>> GetPromedioDiasReservaPago(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetPromedioDiasReservaPagoAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("pagos/reservas-pagos-incompletos")]
    public async Task<ActionResult<IEnumerable<ReservasPagosIncompletosDto>>> GetReservasPagosIncompletos(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetReservasPagosIncompletosAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("pagos/tasa-uso-metodo-pago")]
    public async Task<ActionResult<IEnumerable<TasaUsoMetodoPagoDto>>> GetTasaUsoMetodoPago(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetTasaUsoMetodoPagoAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("pagos/tendencia-mensual-ingresos")]
    public async Task<ActionResult<IEnumerable<TendenciaMensualIngresosDto>>> GetTendenciaMensualIngresos(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetTendenciaMensualIngresosAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // MÉTRICAS - RESERVAS
    [HttpGet("reservas/por-mes")]
    public async Task<ActionResult<IEnumerable<ReservasPorMesDto>>> GetReservasPorMes(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetReservasPorMesAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("reservas/ingresos-promedio-por-tipo-evento")]
    public async Task<ActionResult<IEnumerable<IngresosPromedioPorTipoEventoDto>>> GetIngresosPromedioPorTipoEvento(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetIngresosPromedioPorTipoEventoAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("reservas/adelanto-alto")]
    public async Task<ActionResult<IEnumerable<ReservasAdelantoAltoDto>>> GetReservasAdelantoAlto(
        [FromQuery] decimal porcentajeMinimo = 50, [FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            var resultado = await _reporteService.GetReservasAdelantoAltoAsync(porcentajeMinimo, fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("reservas/duracion-promedio")]
    public async Task<ActionResult<DuracionPromedioReservasDto>> GetDuracionPromedioReservas(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetDuracionPromedioReservasAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("reservas/tasa-conversion-estado")]
    public async Task<ActionResult<TasaConversionEstadoDto>> GetTasaConversionEstado(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetTasaConversionEstadoAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("reservas/distribucion-por-cliente")]
    public async Task<ActionResult<IEnumerable<DistribucionReservasPorClienteDto>>> GetDistribucionReservasPorCliente(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetDistribucionReservasPorClienteAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // MÉTRICAS - SERVICIOS
    [HttpGet("servicios/mas-frecuentes")]
    public async Task<ActionResult<IEnumerable<ServiciosMasFrecuentesDto>>> GetServiciosMasFrecuentes(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] int top = 10)
    {
        try
        {
            var resultado = await _reporteService.GetServiciosMasFrecuentesAsync(fechaInicio, fechaFin, top);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("servicios/variacion-ingresos-mensuales")]
    public async Task<ActionResult<IEnumerable<VariacionIngresosMensualesServicioDto>>> GetVariacionIngresosMensualesServicio(
        [FromQuery] Guid? servicioId, [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetVariacionIngresosMensualesServicioAsync(servicioId, fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("servicios/promedio-items")]
    public async Task<ActionResult<IEnumerable<PromedioItemsPorServicioDto>>> GetPromedioItemsPorServicio(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetPromedioItemsPorServicioAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("servicios/sin-reservas")]
    public async Task<ActionResult<IEnumerable<ServiciosSinReservasDto>>> GetServiciosSinReservas(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetServiciosSinReservasAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("servicios/eventos-cancelados")]
    public async Task<ActionResult<IEnumerable<ServiciosEventosCanceladosDto>>> GetServiciosEventosCancelados(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetServiciosEventosCanceladosAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // RESUMEN EJECUTIVO
    [HttpGet("resumen-ejecutivo")]
    public async Task<ActionResult<ResumenEjecutivoDto>> GetResumenEjecutivo(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = await _reporteService.GetResumenEjecutivoAsync(fechaInicio, fechaFin);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}
