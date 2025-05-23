namespace back_end.Modules.servicios.DTOs
{
    public class ServicioResponseDTO
    {
        public Guid Id { get; set; }
        public string? NombreServicio { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioBase { get; set; }
        public string? TipoEvento { get; set; }
        public string? Imagenes { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public Guid? UsuarioId { get; set; }
        
        public List<ServicioItemDTO> Items { get; set; } = new List<ServicioItemDTO>();
        
        public int TotalItems => Items.Count;
    }
}