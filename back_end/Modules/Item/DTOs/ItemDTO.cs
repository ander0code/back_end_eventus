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
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public string? Preciobase { get; set; }
    }
}