namespace back_end.Modules.servicios.DTOs
{
    public class ServicioUpdateDTO
    {
        public string? NombreServicio { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioBase { get; set; }
        public string? TipoEvento { get; set; }
        public string? Imagenes { get; set; }
    }
}