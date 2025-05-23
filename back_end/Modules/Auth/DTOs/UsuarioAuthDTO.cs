
namespace back_end.Modules.Auth.DTOs
{
    public class UsuarioAuthDTO
    {
        public string Id { get; set; } = null!;
        public string Correo { get; set; } = string.Empty;
        public string ContrasenaHash { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
    }
}