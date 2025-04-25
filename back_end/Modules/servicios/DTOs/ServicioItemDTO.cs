namespace back_end.Modules.servicios.DTOs
{
    public class ServicioItemDTO
    {
        public Guid Id { get; set; }
        public Guid InventarioId { get; set; }
        public int? Cantidad { get; set; }
        public string? NombreItem { get; set; }  
        public string? CategoriaItem { get; set; }
        public int? StockActual { get; set; }  
    }

    public class ServicioItemCreateDTO
    {
        public Guid InventarioId { get; set; }
        public int? Cantidad { get; set; } = 1;
    }

    public class ServicioItemUpdateDTO
    {
        public int? Cantidad { get; set; }
    }
    
    public class ServicioItemsDeleteDTO
    {
        public List<Guid> ItemIds { get; set; } = new List<Guid>();
    }
}