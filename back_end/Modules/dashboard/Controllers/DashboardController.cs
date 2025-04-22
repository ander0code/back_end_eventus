using back_end.Modules.dashboard.DTOs;
using back_end.Modules.dashboard.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.dashboard.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize] // Proteger todos los endpoints del dashboard
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService service, ILogger<DashboardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: api/dashboard/metrics/{correo}
        [HttpGet("metrics/{correo}")]
        public async Task<IActionResult> GetMetrics(string correo)
        {
            try
            {
                _logger.LogInformation("Solicitando métricas del dashboard para usuario: {Correo}", correo);
                var metrics = await _service.GetMetricsAsync(correo);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas del dashboard para usuario: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener métricas", error = ex.Message });
            }
        }

        // GET: api/dashboard/proximas-reservas/{correo}
        [HttpGet("proximas-reservas/{correo}")]
        public async Task<IActionResult> GetProximasReservas(string correo, [FromQuery] int cantidad = 3)
        {
            try
            {
                _logger.LogInformation("Solicitando próximas reservas para usuario: {Correo}, cantidad: {Cantidad}", correo, cantidad);
                var reservas = await _service.GetProximasReservasAsync(correo, cantidad);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener próximas reservas para usuario: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener próximas reservas", error = ex.Message });
            }
        }

        // GET: api/dashboard/actividad-reciente/{correo}
        [HttpGet("actividad-reciente/{correo}")]
        public async Task<IActionResult> GetActividadReciente(string correo, [FromQuery] int cantidad = 10)
        {
            try
            {
                _logger.LogInformation("Solicitando actividad reciente para usuario: {Correo}, cantidad: {Cantidad}", correo, cantidad);
                var actividades = await _service.GetActividadRecienteAsync(correo, cantidad);
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad reciente para usuario: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener actividad reciente", error = ex.Message });
            }
        }
        
        // GET: api/dashboard/{correo}
        // Endpoint consolidado que devuelve toda la información del dashboard
        [HttpGet("{correo}")]
        public async Task<IActionResult> GetAllDashboardData(string correo)
        {
            try
            {
                _logger.LogInformation("Solicitando información completa del dashboard para usuario: {Correo}", correo);
                
                // Ejecutar las consultas secuencialmente para evitar problemas de concurrencia con DbContext
                var metricas = await _service.GetMetricsAsync(correo);
                var proximasReservas = await _service.GetProximasReservasAsync(correo, 3); // 3 próximas reservas
                var actividadReciente = await _service.GetActividadRecienteAsync(correo, 10); // 10 actividades recientes
                
                // Consolidar todos los datos en una respuesta
                var dashboardData = new 
                {
                    Metricas = metricas,
                    ProximasReservas = proximasReservas,
                    ActividadReciente = actividadReciente
                };
                
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información completa del dashboard para usuario: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener información del dashboard", error = ex.Message });
            }
        }
    }
}