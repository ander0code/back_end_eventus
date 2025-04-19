namespace back_end.Modules.servicios.DTOs
{
    public class ServicioCreateDTO
    {
        public string? NombreServicio { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioBase { get; set; }
        public string? TipoEvento { get; set; }
        public string? Imagenes { get; set; }
        
        // Lista de ítems de inventario asociados al servicio
        public List<ServicioItemCreateDTO>? Items { get; set; } = new List<ServicioItemCreateDTO>();
    }
}
