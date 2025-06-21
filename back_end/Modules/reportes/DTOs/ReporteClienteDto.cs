namespace back_end.Modules.reportes.DTOs;

public class ReporteClienteDto
{
    public string Id { get; set; } = null!;
    public string? TipoCliente { get; set; }
    public string? Direccion { get; set; }
    public string? Ruc { get; set; }
    public string? RazonSocial { get; set; }
    public int CantidadReservas { get; set; }
    public decimal? MontoTotalReservas { get; set; }
    public DateTime? UltimaReserva { get; set; }
}

public class ReporteClienteParametrosDto
{
    public string? TipoCliente { get; set; }
    public DateTime? FechaRegistroInicio { get; set; }
    public DateTime? FechaRegistroFin { get; set; }
    public string? Ruc { get; set; }
    public string? RazonSocial { get; set; }
    public int? ReservasMinimas { get; set; }
}