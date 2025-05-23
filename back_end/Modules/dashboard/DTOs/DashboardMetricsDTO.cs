using System;

namespace back_end.Modules.dashboard.DTOs
{
    public class DashboardMetricsDTO
    {

        public int TotalInventarioItems { get; set; }
        public int EventosActivos { get; set; }
        public int TotalClientes { get; set; }
        public int CantidadServicios { get; set; }


        public int ReservasConfirmadasMes { get; set; }
        public int ReservasPendientesMes { get; set; }
        public decimal IngresosEstimadosMes { get; set; }
    }
}