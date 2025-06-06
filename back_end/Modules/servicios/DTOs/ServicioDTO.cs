
namespace back_end.Modules.servicios.DTOs
{
    public class ServicioCreateDTO
    {
        public string NombreServicio { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public List<ServicioItemCreateDTO>? Items { get; set; }
    }

    public class ServicioUpdateDTO
    {
        public string? NombreServicio { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioBase { get; set; }
        public List<ServicioItemCreateDTO>? ItemsToAdd { get; set; }
        public List<Guid>? ItemsToRemove { get; set; }
    }
}