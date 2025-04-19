namespace back_end.Modules.clientes.DTOs;

public class ClienteUpdateDTO
{
    public string? TipoCliente { get; set; }
    public string? Nombre { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}