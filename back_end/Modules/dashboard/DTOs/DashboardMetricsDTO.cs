using System;

namespace back_end.Modules.dashboard.DTOs
{
    public class DashboardMetricsDTO
    {
        // Métricas generales
        public int TotalInventarioItems { get; set; }
        public int EventosActivos { get; set; }
        public int TotalClientes { get; set; }
        public int CantidadServicios { get; set; }

        // Resumen del mes
        public int ReservasConfirmadasMes { get; set; }
        public int ReservasPendientesMes { get; set; }
        public decimal IngresosEstimadosMes { get; set; }
    }
}