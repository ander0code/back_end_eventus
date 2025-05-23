namespace back_end.Modules.organizador.DTOs
{
    public class UsuarioResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Correo { get; set; }
        public string? Celular { get; set; }
    }

    public class UsuarioCreateDTO
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Correo { get; set; }
        public string? Celular { get; set; }
    }

    public class UsuarioUpdateDTO
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Celular { get; set; }
    }
}