using back_end.Modules.organizador.Models;

namespace back_end.Modules.organizador.DTOs
{
    public class OrganizadorResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? NombreNegocio { get; set; }
        public string? UsuarioId { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Correo { get; set; }
        public string? Celular { get; set; }
    }

    public class OrganizadorCreateDTO
    {
        public string? NombreNegocio { get; set; }
        public string? Contrasena { get; set; }
        // Datos del usuario asociado
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Correo { get; set; }
        public string? Celular { get; set; }
    }

    public class OrganizadorUpdateDTO
    {
        public string? NombreNegocio { get; set; }
        public string? Contrasena { get; set; }
    }
}