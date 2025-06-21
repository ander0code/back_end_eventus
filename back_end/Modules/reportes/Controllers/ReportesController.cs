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
    }    // MÉTRICAS UNIFICADAS - CLIENTES
    [HttpGet("clientes")]
    public async Task<ActionResult<object>> GetReportesClientes(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = new
            {
                ClientesNuevosPorMes = await _reporteService.GetClientesNuevosPorMesAsync(fechaInicio, fechaFin),
                PromedioAdelantoPorCliente = await _reporteService.GetPromedioAdelantoPorClienteAsync(null),
                TasaRetencionClientes = await _reporteService.GetTasaRetencionClientesAsync(fechaInicio, fechaFin)
            };
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // MÉTRICAS UNIFICADAS - INVENTARIO/ITEMS
    [HttpGet("items")]
    public async Task<ActionResult<object>> GetReportesItems(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] int top = 10)
    {
        try
        {
            var resultado = new
            {
                ItemsMasUtilizados = await _reporteService.GetItemsMasUtilizadosAsync(fechaInicio, fechaFin, top),
                StockPromedioPorTipoServicio = await _reporteService.GetStockPromedioPorTipoServicioAsync(fechaInicio, fechaFin),
                TasaDisponibilidad = await _reporteService.GetTasaDisponibilidadAsync()
            };
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // MÉTRICAS UNIFICADAS - PAGOS
    [HttpGet("pagos")]
    public async Task<ActionResult<object>> GetReportesPagos(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = new
            {
                MontoPromedioPorPago = await _reporteService.GetMontoPromedioPorPagoAsync(fechaInicio, fechaFin),
                PromedioDiasReservaPago = await _reporteService.GetPromedioDiasReservaPagoAsync(fechaInicio, fechaFin),
                ReservasPagosIncompletos = await _reporteService.GetReservasPagosIncompletosAsync(fechaInicio, fechaFin),
                TasaUsoMetodoPago = await _reporteService.GetTasaUsoMetodoPagoAsync(fechaInicio, fechaFin),
                TendenciaMensualIngresos = await _reporteService.GetTendenciaMensualIngresosAsync(fechaInicio, fechaFin)
            };
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }    // MÉTRICAS UNIFICADAS - RESERVAS
    [HttpGet("reservas")]
    public async Task<ActionResult<object>> GetReportesReservas(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var resultado = new
            {
                ReservasPorMes = await _reporteService.GetReservasPorMesAsync(fechaInicio, fechaFin),
                IngresosPromedioPorTipoEvento = await _reporteService.GetIngresosPromedioPorTipoEventoAsync(fechaInicio, fechaFin),
                ReservasAdelantoAlto = await _reporteService.GetReservasAdelantoAltoAsync(fechaInicio: fechaInicio, fechaFin: fechaFin),
                DuracionPromedioReservas = await _reporteService.GetDuracionPromedioReservasAsync(fechaInicio, fechaFin),
                TasaConversionEstado = await _reporteService.GetTasaConversionEstadoAsync(fechaInicio, fechaFin),
                DistribucionReservasPorCliente = await _reporteService.GetDistribucionReservasPorClienteAsync(fechaInicio, fechaFin)
            };
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }// MÉTRICAS UNIFICADAS - SERVICIOS
    [HttpGet("servicios")]
    public async Task<ActionResult<object>> GetReportesServicios(
        [FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] int top = 10)
    {
        try
        {
            var resultado = new
            {
                ServiciosMasFrecuentes = await _reporteService.GetServiciosMasFrecuentesAsync(fechaInicio, fechaFin, top),
                VariacionIngresosMensualesServicio = await _reporteService.GetVariacionIngresosMensualesServicioAsync(null, fechaInicio, fechaFin),
                PromedioItemsPorServicio = await _reporteService.GetPromedioItemsPorServicioAsync(fechaInicio, fechaFin),
                ServiciosSinReservas = await _reporteService.GetServiciosSinReservasAsync(fechaInicio, fechaFin),
                ServiciosEventosCancelados = await _reporteService.GetServiciosEventosCanceladosAsync(fechaInicio, fechaFin)
            };
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
