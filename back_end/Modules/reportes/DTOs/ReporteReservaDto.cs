namespace back_end.Modules.reportes.DTOs;

public class ReporteReservaDto
{
    public string Id { get; set; } = null!;
    public string? NombreEvento { get; set; }
    public DateOnly? FechaEjecucion { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public string? Descripcion { get; set; }
    public string? Estado { get; set; }
    public decimal? PrecioTotal { get; set; }
    public string? TipoEvento { get; set; }
    public string? ClienteRazonSocial { get; set; }
    public string? ServicioNombre { get; set; }
    public double? PrecioAdelanto { get; set; }
    public int CantidadPagos { get; set; }
}

public class ReporteReservaParametrosDto
{
    public DateTime? FechaEjecucionInicio { get; set; }
    public DateTime? FechaEjecucionFin { get; set; }
    public DateTime? FechaRegistroInicio { get; set; }
    public DateTime? FechaRegistroFin { get; set; }
    public string? Estado { get; set; }
    public string? TipoEvento { get; set; }
    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
    public string? ClienteId { get; set; }
}