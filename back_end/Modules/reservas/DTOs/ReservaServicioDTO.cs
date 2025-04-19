namespace back_end.Modules.reservas.DTOs;

public class ReservaServicioDTO
{
    public Guid ServicioId { get; set; }
    public int? CantidadItems { get; set; }
    public decimal? Precio { get; set; }
}