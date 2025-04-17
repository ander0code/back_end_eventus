namespace back_end.Modules.servicios.DTOs
{
    public class ServicioResponseDTO
    {
        public int Id { get; set; }
        public string? NombreServicio { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioBase { get; set; }
        public string? TipoEvento { get; set; }
        public string? Imagenes { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int? UsuarioId { get; set; }
    }
}