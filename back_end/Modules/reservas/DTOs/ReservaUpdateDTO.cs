namespace back_end.Modules.reservas.DTOs
{
    public class ReservaUpdateDTO
    {
        public string? NombreEvento { get; set; }
        public DateOnly? FechaEvento { get; set; }
        public string? HoraEvento { get; set; }
        public string? TipoEvento { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
        public decimal? PrecioTotal { get; set; }

        public List<ServicioToAddDTO>? ItemsToAdd { get; set; }
        
        public List<Guid>? ItemsToRemove { get; set; }
    }

    public class ServicioToAddDTO
    {
        public Guid ServicioId { get; set; }
    }
}