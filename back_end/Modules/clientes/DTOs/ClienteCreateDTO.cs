namespace back_end.Modules.clientes.DTOs;

public class ClienteCreateDTO
{
    public string? TipoCliente { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? CorreoElectronico { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}