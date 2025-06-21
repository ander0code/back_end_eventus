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

    [HttpPost("items")]
    public async Task<ActionResult<IEnumerable<ReporteItemDto>>> GetReporteItems([FromBody] ReporteItemParametrosDto parametros)
    {
        try
        {
            var resultado = await _reporteService.GetReporteItemsAsync(parametros);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpPost("pagos")]
    public async Task<ActionResult<IEnumerable<ReportePagoDto>>> GetReportePagos([FromBody] ReportePagoParametrosDto parametros)
    {
        try
        {
            var resultado = await _reporteService.GetReportePagosAsync(parametros);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpPost("clientes")]
    public async Task<ActionResult<IEnumerable<ReporteClienteDto>>> GetReporteClientes([FromBody] ReporteClienteParametrosDto parametros)
    {
        try
        {
            var resultado = await _reporteService.GetReporteClientesAsync(parametros);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }    [HttpPost("reservas")]
    public async Task<ActionResult<IEnumerable<ReporteReservaDto>>> GetReporteReservas([FromBody] ReporteReservaParametrosDto parametros)
    {
        try
        {
            var resultado = await _reporteService.GetReporteReservasAsync(parametros);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpPost("servicios")]
    public async Task<ActionResult<IEnumerable<ReporteServicioDto>>> GetReporteServicios([FromBody] ReporteServicioParametrosDto parametros)
    {
        try
        {
            var resultado = await _reporteService.GetReporteServiciosAsync(parametros);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}