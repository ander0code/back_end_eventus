namespace back_end.Modules.servicios.DTOs
{
    public class ServicioUpdateDTO
    {
        public string? NombreServicio { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioBase { get; set; }
        public string? TipoEvento { get; set; }
        public string? Imagenes { get; set; }
        
        // Items para agregar o actualizar al servicio
        // Si un item ya existe (mismo InventarioId), se actualizará la cantidad
        public List<ServicioItemCreateDTO>? ItemsToAdd { get; set; }
        
        // IDs de items a eliminar del servicio
        public List<Guid>? ItemsToRemove { get; set; }
    }
}