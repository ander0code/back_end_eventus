namespace back_end.Modules.Auth.DTOs
{
    // DTOs para login
    public class AuthRequestDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }    public class AuthResponseDTO
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreNegocio { get; set; } = string.Empty;
    }

    // DTOs para registro
    public class RegisterRequestDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public string? Telefono { get; set; }
    }    public class RegisterResponseDTO
    {
        public string UserId { get; set; } = string.Empty; // Cambiado de Guid a string
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Message { get; set; } = "Usuario registrado correctamente";
    }
}