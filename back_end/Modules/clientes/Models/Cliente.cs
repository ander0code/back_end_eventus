namespace back_end.Modules.clientes.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.organizador.Models;

public partial class Cliente
{
    public string Id { get; set; } = null!;

    public string? TipoCliente { get; set; }

    public string? Direccion { get; set; }

    public string? Ruc { get; set; }

    public string? RazonSocial { get; set; }

    public string? UsuarioId { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual Usuario? Usuario { get; set; }
}
