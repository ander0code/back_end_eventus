using System;

namespace back_end.Modules.dashboard.DTOs
{    public class ActividadRecienteItemDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Tipo { get; set; }  
        public string? Nombre { get; set; } 
        public DateTime FechaRegistro { get; set; }
        public string? TiempoTranscurrido { get; set; }  
    }

    public class ActividadRecienteDTO
    {
        public List<ActividadRecienteItemDTO> Actividades { get; set; } = new List<ActividadRecienteItemDTO>();
    }
}