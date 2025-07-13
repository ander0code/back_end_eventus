namespace back_end.Modules.reportes.DTOs;

// Métricas para inventario/items
public class ItemsMasUtilizadosDto
{
    public string? InventarioId { get; set; }
    public string? NombreItem { get; set; }
    public int TotalCantidadUtilizada { get; set; }
    public int FrecuenciaUso { get; set; }
    public decimal PromedioUsoPorServicio { get; set; }
}

public class StockPromedioPorTipoServicioDto
{
    public string? ServicioId { get; set; }
    public string? NombreServicio { get; set; }
    public decimal StockPromedioUtilizado { get; set; }
    public int CantidadDetalles { get; set; }
}

public class TasaDisponibilidadDto
{
    public string? InventarioId { get; set; }
    public string? NombreItem { get; set; }
    public int Stock { get; set; }
    public int StockDisponible { get; set; }
    public decimal TasaDisponibilidadPorc { get; set; }
}