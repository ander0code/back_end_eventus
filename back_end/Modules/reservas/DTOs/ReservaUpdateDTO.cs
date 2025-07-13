namespace back_end.Modules.reservas.DTOs
{
    public class ReservaUpdateDTO
    {
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEjecucion { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string? ServicioId { get; set; }
        public double? PrecioAdelanto { get; set; }
        public string? TipoEventoNombre { get; set; }
    }
}