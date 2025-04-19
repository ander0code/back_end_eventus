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
        
        // Colección de items asociados al servicio
        public List<ServicioItemDTO> Items { get; set; } = new List<ServicioItemDTO>();
        
        // Cantidad total de items asignados al servicio (para visualización rápida)
        public int TotalItems => Items.Sum(i => i.Cantidad ?? 0);
    }
}