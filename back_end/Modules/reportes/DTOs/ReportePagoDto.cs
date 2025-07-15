namespace back_end.Modules.reportes.DTOs;

// Métricas para pagos
public class MontoPromedioPorPagoDto
{
    public string? TipoPago { get; set; }
    public decimal MontoPromedio { get; set; }
    public int CantidadPagos { get; set; }
    public decimal MontoTotal { get; set; }
}

public class PromediotDiasReservaPagoDto
{
    public decimal PromedioDias { get; set; }
    public int CantidadReservasConPagos { get; set; }
    public decimal DiasMinimo { get; set; }
    public decimal DiasMaximo { get; set; }
}

public class ReservasPagosIncompletosDto
{
    public string ReservaId { get; set; } = null!;
    public string? NombreEvento { get; set; }
    public string? ClienteRazonSocial { get; set; }
    public decimal PrecioTotal { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal MontoPendiente { get; set; }
    public decimal PorcentajePagado { get; set; }
}

public class TasaUsoMetodoPagoDto
{
    public string? TipoPago { get; set; }
    public int CantidadUsos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PorcentajeUso { get; set; }
}

public class TendenciaMensualIngresosDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = null!;
    public decimal MontoTotal { get; set; }
    public int CantidadPagos { get; set; }
    public decimal MontoPromedio { get; set; }
}