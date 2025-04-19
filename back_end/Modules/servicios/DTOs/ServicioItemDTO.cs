namespace back_end.Modules.servicios.DTOs
{
    public class ServicioItemDTO
    {
        public Guid Id { get; set; }
        public Guid InventarioId { get; set; }
        public int? Cantidad { get; set; }
        public string? NombreItem { get; set; }  // Para mostrar información del inventario
        public string? CategoriaItem { get; set; }
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
}