namespace back_end.Modules.reservas.DTOs
{
    public class ReservaUpdateDTO
    {
        public string? Estado { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioTotal { get; set; }
        public string? Observaciones { get; set; }
    }
}