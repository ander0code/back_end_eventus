namespace back_end.Modules.usuarios.DTOs
{
    public class UsuarioUpdateDTO
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public bool? Verificado { get; set; }
    }
}
