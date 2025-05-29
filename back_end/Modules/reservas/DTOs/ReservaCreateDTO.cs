namespace back_end.Modules.reservas.DTOs
{    public class ReservaCreateDTO
    {
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEjecucion { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string? ClienteId { get; set; }
        // Cambiar TipoEventoId por TipoEventoNombre
        public string? TipoEventoNombre { get; set; }
        public Guid? ServicioId { get; set; }
        public double? PrecioAdelanto { get; set; }
    }
}