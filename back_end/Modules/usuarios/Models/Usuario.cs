using System;
using System.Collections.Generic;

using back_end.Modules.inventario.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.servicios.Models;

namespace back_end.Modules.usuarios.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public string ContrasenaHash { get; set; } = null!;

    public bool? Verificado { get; set; }

    public string? TokenVerificacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
