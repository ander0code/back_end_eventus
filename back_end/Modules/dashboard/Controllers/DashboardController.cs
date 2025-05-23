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
        

        [HttpGet("{correo}")]
        public async Task<IActionResult> GetAllDashboardData(string correo)
        {            try
            {
                _logger.LogInformation("Solicitando información completa del dashboard para usuario: {Correo}", correo);
                

                var metricas = await _service.GetMetricsAsync(correo);
                var proximasReservas = await _service.GetProximasReservasAsync(correo, 4);
                var actividadReciente = await _service.GetActividadRecienteAsync(correo, 10); 
                

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