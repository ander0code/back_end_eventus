namespace back_end.Modules.reservas.DTOs
{
    public class ReservaCreateDTO
    {
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEvento { get; set; }
        public string? HoraEvento { get; set; }
        public string? TipoEvento { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public decimal? PrecioTotal { get; set; }
        public Guid? ClienteId { get; set; }
        public string? NombreCliente { get; set; }
        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }

        public List<Guid>? Servicios { get; set; }
    }
}