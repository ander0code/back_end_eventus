namespace back_end.Modules.servicios.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.usuarios.Models;

public partial class Servicio
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Categoria { get; set; }

    public string? Descripcion { get; set; }

    public decimal? PrecioBase { get; set; }

    public string? Imagenes { get; set; }

    public Guid UsuarioId { get; set; }

    public virtual ICollection<ReservaServicio> ReservaServicios { get; set; } = new List<ReservaServicio>();

    public virtual ICollection<ServicioItem> ServicioItems { get; set; } = new List<ServicioItem>();

    public virtual Usuario Usuario { get; set; } = null!;
}
