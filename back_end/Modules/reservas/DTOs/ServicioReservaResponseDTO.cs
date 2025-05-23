namespace back_end.Modules.reservas.DTOs
{
    public class ServicioReservaResponseDTO
    {
        public Guid ServicioId { get; set; }
        public string? NombreServicio { get; set; }
        public int CantidadItems { get; set; }
        public decimal Precio { get; set; }
        public string? Descripcion { get; set; }
    }
}