using System;
using System.Collections.Generic;

using back_end.Modules.reservas.Models;
using back_end.Modules.usuarios.Models;
namespace back_end.Modules.servicios.Models;

public partial class Servicio
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public string? NombreServicio { get; set; }

    public string? Descripcion { get; set; }

    public decimal? PrecioBase { get; set; }

    public string? TipoEvento { get; set; }

    public string? Imagenes { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<ServicioInventario> ServicioInventarios { get; set; } = new List<ServicioInventario>();

    public virtual Usuario? Usuario { get; set; }
}
