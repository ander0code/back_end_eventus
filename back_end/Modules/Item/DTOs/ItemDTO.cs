namespace back_end.Modules.Item.DTOs
{
    public class ItemCreateDTO
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public string? Preciobase { get; set; }
    }

    public class ItemUpdateDTO
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public string? Preciobase { get; set; }
    }

    public class ItemResponseDTO
    {
        public string? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public int StockDisponible { get; set; }
        public string? Preciobase { get; set; }
        public int ItemsEnUso { get; set; } // Simplemente un contador de cuántos están en uso
    }

    public class ItemListResponseDTO
    {
        public string? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public int StockDisponible { get; set; }
        public string? Preciobase { get; set; }
        public int ItemsEnUso { get; set; } // Simplemente un contador de cuántos están en uso
    }
}