namespace back_end.Modules.reservas.DTOs
{
    public class ReservaResponseDTO
    {
        public Guid Id { get; set; }
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEvento { get; set; }
        public string? HoraEvento { get; set; }
        public string? TipoEvento { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string? NombreCliente { get; set; }
        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public Guid? ServicioId { get; set; }
        public DateTime? FechaReserva { get; set; }
        public string? Observaciones { get; set; }
    }
}