using System;

namespace back_end.Modules.dashboard.DTOs
{    public class ProximaReservaDTO
    {
        public string? Id { get; set; }
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEjecucion { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
    }
    
    public class ProximasReservasDTO
    {
        public List<ProximaReservaDTO> Reservas { get; set; } = new List<ProximaReservaDTO>();
    }
}