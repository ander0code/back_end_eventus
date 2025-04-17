namespace back_end.Modules.reservas.DTOs
{
    public class ReservaResponseDTO
    {
        public int Id { get; set; }
        public string? NombreCliente { get; set; }
        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public int? ServicioId { get; set; }
        public DateOnly? FechaEvento { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaReserva { get; set; }
        public string? Observaciones { get; set; }
    }
}