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
        
        public List<ServicioReservaResponseDTO> Servicios { get; set; } = new List<ServicioReservaResponseDTO>();
        
        public DateTime? FechaReserva { get; set; }
    }
}