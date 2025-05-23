using back_end.Modules.organizador.Models;

namespace back_end.Modules.clientes.DTOs
{    public class ClienteCreateDTO
    {
        public string? TipoCliente { get; set; }
        public string? Nombre { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ruc { get; set; }
        public string? RazonSocial { get; set; }
    }    public class ClienteUpdateDTO
    {
        public string? TipoCliente { get; set; }
        public string? Nombre { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ruc { get; set; }
        public string? RazonSocial { get; set; }
    }    public class ClienteResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? TipoCliente { get; set; }
        public string? Direccion { get; set; }
        public string? Ruc { get; set; }
        public string? RazonSocial { get; set; }
        public string? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string? CorreoUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Telefono { get; set; }
        public int TotalReservas { get; set; }
        public DateOnly? UltimaFechaReserva { get; set; }
    }
}