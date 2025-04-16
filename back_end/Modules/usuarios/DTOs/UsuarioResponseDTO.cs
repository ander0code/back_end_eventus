namespace back_end.Modules.usuarios.DTOs
{
    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string Correo { get; set; } = null!;
        public string? Telefono { get; set; }
        public bool? Verificado { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }
}