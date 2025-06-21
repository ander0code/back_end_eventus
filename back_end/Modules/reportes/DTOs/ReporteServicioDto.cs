namespace back_end.Modules.reportes.DTOs;

// Métricas para servicios
public class ServiciosMasFrecuentesDto
{
    public Guid ServicioId { get; set; }
    public string? NombreServicio { get; set; }
    public int CantidadReservas { get; set; }
    public decimal PorcentajeUso { get; set; }
    public decimal IngresoTotal { get; set; }
    public decimal IngresoPromedio { get; set; }
}

public class VariacionIngresosMensualesServicioDto
{
    public Guid ServicioId { get; set; }
    public string? NombreServicio { get; set; }
    public int Año { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = null!;
    public decimal MontoMensual { get; set; }
    public int CantidadReservas { get; set; }
    public decimal VariacionPorc { get; set; }
}

public class PromedioItemsPorServicioDto
{
    public Guid ServicioId { get; set; }
    public string? NombreServicio { get; set; }
    public decimal PromedioItemsUsados { get; set; }
    public int TotalDetalles { get; set; }
    public int CantidadReservas { get; set; }
}

public class ServiciosSinReservasDto
{
    public Guid ServicioId { get; set; }
    public string? NombreServicio { get; set; }
    public string? Descripcion { get; set; }
    public decimal? PrecioBase { get; set; }
    public int DiasInactivo { get; set; }
}

public class ServiciosEventosCanceladosDto
{
    public Guid ServicioId { get; set; }
    public string? NombreServicio { get; set; }
    public int TotalReservas { get; set; }
    public int ReservasCanceladas { get; set; }
    public decimal PorcentajeCancelacion { get; set; }
    public decimal MontoPerdidasCancelacion { get; set; }
}