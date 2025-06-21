namespace back_end.Modules.reportes.DTOs;

public class ReportePagoDto
{
    public string Id { get; set; } = null!;
    public string? IdReserva { get; set; }
    public string? TipoPago { get; set; }
    public string? Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string? NombreEvento { get; set; }
    public string? ClienteRazonSocial { get; set; }
}

public class ReportePagoParametrosDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? TipoPago { get; set; }
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }
    public string? ClienteId { get; set; }
}