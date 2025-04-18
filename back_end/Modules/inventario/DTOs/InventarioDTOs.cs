namespace back_end.Modules.inventario.DTOs
{
    public class InventarioResponseDTO
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public string? Categoria { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string? NombreUsuario { get; set; }
    }

    public class InventarioCreateDTO
    {
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public string? Categoria { get; set; }
    }

    public class InventarioUpdateDTO
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? Stock { get; set; }
        public string? Categoria { get; set; }
    }
}