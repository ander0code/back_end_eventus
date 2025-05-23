using Microsoft.EntityFrameworkCore;

using back_end.Modules.clientes.Models;
using back_end.Modules.Item.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.servicios.Models;
using back_end.Modules.organizador.Models;
using back_end.Modules.pagos.Models;

namespace back_end.Core.Data;
public partial class DbEventusContext : DbContext
{
    public DbEventusContext()
    {
    }

    public DbEventusContext(DbContextOptions<DbEventusContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetalleServicio> DetalleServicios { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Organizador> Organizadors { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<TipoPago> TipoPagos { get; set; }

    public virtual DbSet<TiposEvento> TiposEventos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        if (!optionsBuilder.IsConfigured)
        {

        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clientes__3213E83FA33B7E55");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Ruc)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("RUC");
            entity.Property(e => e.TipoCliente)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Clientes__Usuari__398D8EEE");
        });

        modelBuilder.Entity<DetalleServicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DetalleS__3213E83FFEF6026C");

            entity.ToTable("DetalleServicio");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.InventarioId).HasColumnName("InventarioID");
            entity.Property(e => e.PrecioActual)
                .HasMaxLength(10)
                .HasColumnName("precioActual");
            entity.Property(e => e.ServicioId).HasColumnName("ServicioID");

            entity.HasOne(d => d.Inventario).WithMany(p => p.DetalleServicios)
                .HasForeignKey(d => d.InventarioId)
                .HasConstraintName("FK__DetalleSe__Inven__5070F446");

            entity.HasOne(d => d.Servicio).WithMany(p => p.DetalleServicios)
                .HasForeignKey(d => d.ServicioId)
                .HasConstraintName("FK__DetalleSe__Servi__4F7CD00D");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Items__3213E83F313E09DD");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Preciobase)
                .HasMaxLength(10)
                .HasColumnName("preciobase");
        });

        modelBuilder.Entity<Organizador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Organiza__3213E83F1C25C70E");

            entity.ToTable("Organizador");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NombreNegocio)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Organizadors)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Organizad__Usuar__3C69FB99");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pagos__3213E83FAC3B547F");

            entity.ToTable("pagos");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.IdReserva)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id_reserva");
            entity.Property(e => e.IdTipoPago)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id_tipoPago");
            entity.Property(e => e.Monto)
                .HasMaxLength(10)
                .HasColumnName("monto");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdReserva)
                .HasConstraintName("FK__pagos__id_reserv__4BAC3F29");

            entity.HasOne(d => d.IdTipoPagoNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdTipoPago)
                .HasConstraintName("FK__pagos__id_tipoPa__4CA06362");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reserva__3213E83F79988A45");

            entity.ToTable("Reserva");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.ClienteId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ClienteID");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreEvento)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PrecioTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServicioId).HasColumnName("ServicioID");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK__Reserva__Cliente__45F365D3");

            entity.HasOne(d => d.Servicio).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.ServicioId)
                .HasConstraintName("FK__Reserva__Servici__46E78A0C");

            entity.HasOne(d => d.TiposEventoNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.TiposEvento)
                .HasConstraintName("FK__Reserva__TiposEv__44FF419A");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Servicio__3213E83FE922B26C");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PrecioBase).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<TipoPago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tipoPago__3213E83F0B4BD09E");

            entity.ToTable("tipoPago");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(10)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TiposEvento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TiposEve__3213E83FADDDB3B1");

            entity.ToTable("TiposEvento");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83FEEF59A18");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
