namespace back_end.Modules.reportes.DTOs;

public class ReporteServicioDto
{
    public Guid Id { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? PrecioBase { get; set; }
    public int CantidadReservas { get; set; }
    public int CantidadDetalles { get; set; }
    public decimal? IngresosTotales { get; set; }
    public DateTime? UltimaReserva { get; set; }
}

public class ReporteServicioParametrosDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
    public string? Nombre { get; set; }
    public int? ReservasMinimas { get; set; }
}