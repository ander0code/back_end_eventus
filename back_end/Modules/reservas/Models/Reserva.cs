using System;
using System.Collections.Generic;


using back_end.Modules.servicios.Models;
using back_end.Modules.usuarios.Models;

namespace back_end.Modules.reservas.Models;

public partial class Reserva
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public string? NombreCliente { get; set; }

    public string? CorreoCliente { get; set; }

    public string? TelefonoCliente { get; set; }

    public int? ServicioId { get; set; }

    public DateOnly? FechaEvento { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaReserva { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual Servicio? Servicio { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
