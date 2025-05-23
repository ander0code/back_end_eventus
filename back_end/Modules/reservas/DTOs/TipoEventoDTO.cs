namespace back_end.Modules.reservas.DTOs
{
    public class TipoEventoResponseDTO
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    public class TipoEventoCreateDTO
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    public class TipoEventoUpdateDTO
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    public class TipoEventoDTO
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}