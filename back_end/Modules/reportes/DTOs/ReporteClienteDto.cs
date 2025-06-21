namespace back_end.Modules.reportes.DTOs;

// Métricas para clientes
public class ClientesNuevosPorMesDto
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = null!;
    public int CantidadClientesNuevos { get; set; }
}

public class PromedioAdelantoPorClienteDto
{
    public string ClienteId { get; set; } = null!;
    public string? RazonSocial { get; set; }
    public decimal PromedioAdelantoPorc { get; set; }
    public int CantidadReservas { get; set; }
}

public class TasaRetencionClientesDto
{
    public int TotalClientes { get; set; }
    public int ClientesConMultiplesReservas { get; set; }
    public decimal PorcentajeMultiplesReservas { get; set; }
    public decimal TasaRetencion { get; set; }
}