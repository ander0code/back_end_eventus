namespace back_end.Modules.reportes.DTOs;

// DTO para parámetros generales de reportes
public class ParametrosGeneralesReporteDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? ClienteId { get; set; }
    public Guid? ServicioId { get; set; }
    public string? TipoEvento { get; set; }
    public string? Estado { get; set; }
    public int? Top { get; set; } = 10;
}

// DTO para resumen ejecutivo de todas las métricas
public class ResumenEjecutivoDto
{
    // Métricas de clientes
    public int TotalClientes { get; set; }
    public int ClientesNuevosUltimoMes { get; set; }
    public decimal TasaRetencionClientes { get; set; }

    // Métricas de reservas
    public int TotalReservas { get; set; }
    public int ReservasUltimoMes { get; set; }
    public decimal IngresosTotales { get; set; }
    public decimal IngresosUltimoMes { get; set; }
    public decimal TasaConversionReservas { get; set; }

    // Métricas de pagos
    public decimal MontoPromedioReserva { get; set; }
    public decimal PorcentajePagosCompletos { get; set; }
    public decimal PromedioDiasPago { get; set; }

    // Métricas de servicios
    public int TotalServicios { get; set; }
    public int ServiciosActivos { get; set; }
    public string? ServicioMasFrecuente { get; set; }

    // Métricas de inventario
    public int TotalItems { get; set; }
    public decimal TasaDisponibilidadPromedio { get; set; }
    public string? ItemMasUtilizado { get; set; }

    // Período del reporte
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaGeneracion { get; set; }
}
