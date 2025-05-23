namespace back_end.Modules.reservas.DTOs
{
    public class ReservaResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEjecucion { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string? ClienteId { get; set; }
        public string? NombreCliente { get; set; }
        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public Guid? TipoEventoId { get; set; }
        public string? TipoEventoNombre { get; set; }
        public Guid? ServicioId { get; set; }
        public string? NombreServicio { get; set; }
        public double? PrecioAdelanto { get; set; }
        public decimal? TotalPagado { get; set; }
        public DateTime? UltimoPago { get; set; }
    }
}