using System;

namespace back_end.Modules.Auth.DTOs
{
    public class UsuarioAuthDTO
    {
        public int Id { get; set; }
        public required string Correo { get; set; }
        public required string ContrasenaHash { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public bool? Verificado { get; set; }
    }
}