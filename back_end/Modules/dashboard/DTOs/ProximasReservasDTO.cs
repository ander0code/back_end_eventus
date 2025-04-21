using System;

namespace back_end.Modules.dashboard.DTOs
{
    public class ProximaReservaDTO
    {
        public Guid? Id { get; set; }
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEvento { get; set; }
        public string? HoraEvento { get; set; }
        public string? Descripcion { get; set; }
    }
    
    public class ProximasReservasDTO
    {
        public List<ProximaReservaDTO> Reservas { get; set; } = new List<ProximaReservaDTO>();
    }
}