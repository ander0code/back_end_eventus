namespace back_end.Modules.reportes.DTOs;

public class ReporteItemDto
{
    public Guid Id { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int? Stock { get; set; }
    public int StockDisponible { get; set; }
    public string? Preciobase { get; set; }
    public int CantidadServicios { get; set; }
}

public class ReporteItemParametrosDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    public string? Nombre { get; set; }
}