namespace back_end.Modules.reportes.DTOs;

// Métricas para reservas
public class ReservasPorMesDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = null!;
    public int CantidadReservas { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal MontoPromedio { get; set; }
}

public class IngresosPromedioPorTipoEventoDto
{
    public string? TipoEvento { get; set; }
    public decimal IngresoPromedio { get; set; }
    public int CantidadReservas { get; set; }
    public decimal IngresoTotal { get; set; }
    public decimal IngresoMinimo { get; set; }
    public decimal IngresoMaximo { get; set; }
}

public class ReservasAdelantoAltoDto
{
    public string ReservaId { get; set; } = null!;
    public string? NombreEvento { get; set; }
    public string? ClienteRazonSocial { get; set; }
    public decimal PrecioTotal { get; set; }
    public decimal MontoAdelanto { get; set; }
    public decimal PorcentajeAdelanto { get; set; }
}

public class DuracionPromedioReservasDto
{
    public decimal DuracionPromedioDias { get; set; }
    public int CantidadReservas { get; set; }
    public decimal DuracionMinimaDias { get; set; }
    public decimal DuracionMaximaDias { get; set; }
}

public class TasaConversionEstadoDto
{
    public int ReservasPendientes { get; set; }
    public int ReservasConfirmadas { get; set; }
    public int ReservasCanceladas { get; set; }
    public int ReservasFinalizadas { get; set; }
    public decimal TasaConversionPendienteConfirmado { get; set; }
    public decimal TasaCancelacion { get; set; }
    public decimal TasaFinalizacion { get; set; }
}

public class DistribucionReservasPorClienteDto
{
    public string ClienteId { get; set; } = null!;
    public string? RazonSocial { get; set; }
    public int TotalReservas { get; set; }
    public int ReservasUltimosTresMeses { get; set; }
    public decimal MontoTotalReservas { get; set; }
    public DateTime? UltimaReserva { get; set; }
}