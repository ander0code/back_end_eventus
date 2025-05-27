namespace back_end.Modules.servicios.DTOs
{
    public class DetalleServicioDTO
    {
        public Guid Id { get; set; }
        public Guid? InventarioId { get; set; }
        public double? Cantidad { get; set; }
        public string? NombreItem { get; set; }
        public string? Estado { get; set; }
        public DateTime? Fecha { get; set; }
        public string? PrecioActual { get; set; }
        public int StockDisponible { get; set; } 
    }

    public class DetalleServicioCreateDTO
    {
        public Guid InventarioId { get; set; }
        public double? Cantidad { get; set; } = 1;
        // El estado está limitado a 10 caracteres en la base de datos
        public string? Estado { get; set; }
        public string? PrecioActual { get; set; }
    }

    public class DetalleServicioUpdateDTO
    {
        public double? Cantidad { get; set; }
        // El estado está limitado a 10 caracteres en la base de datos
        public string? Estado { get; set; }
        public string? PrecioActual { get; set; }
    }
    
    public class DetalleServicioDeleteDTO
    {
        public List<Guid> ItemIds { get; set; } = new List<Guid>();
    }
    
    // Mantenemos las clases antiguas para compatibilidad
    public class ServicioItemDTO : DetalleServicioDTO { }
    public class ServicioItemCreateDTO : DetalleServicioCreateDTO { }
    public class ServicioItemUpdateDTO : DetalleServicioUpdateDTO { }
    public class ServicioItemsDeleteDTO : DetalleServicioDeleteDTO { }
}