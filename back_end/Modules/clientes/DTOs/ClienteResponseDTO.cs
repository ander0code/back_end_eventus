namespace back_end.Modules.clientes.DTOs;

public class ClienteResponseDTO
{
    public Guid Id { get; set; }
    public string? TipoCliente { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? CorreoElectronico { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaRegistro { get; set; }
}