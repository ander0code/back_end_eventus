using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using back_end.Modules.usuarios.Models;
using back_end.Modules.servicios.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.inventario.Models;

namespace back_end.Core.Data
{

public partial class DbEventusContext : DbContext
{
    public DbEventusContext()
    {
    }

    public DbEventusContext(DbContextOptions<DbEventusContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<ServicioInventario> ServicioInventarios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // Solo configurar si no está ya configurado por la inyección de dependencias
    if (!optionsBuilder.IsConfigured)
    {
        // Esto es solo un fallback y no se usará en aplicaciones normales
        // donde el DbContext se inyecta correctamente
    }
}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventar__3213E83FC20FBD1F");

            entity.ToTable("Inventario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreItem).HasMaxLength(100);
            entity.Property(e => e.PrecioDia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PrecioMes).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PrecioSemana).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Inventarios)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Inventari__usuar__403A8C7D");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3213E83F6BD9968C");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EstadoEnvio).HasDefaultValue(false);
            entity.Property(e => e.FechaEnvio).HasColumnType("datetime");
            entity.Property(e => e.ReservaId).HasColumnName("reserva_id");
            entity.Property(e => e.TipoNotificacion).HasMaxLength(50);

            entity.HasOne(d => d.Reserva).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.ReservaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Notificac__reser__4D94879B");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reservas__3213E83FB9419F2A");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CorreoCliente).HasMaxLength(150);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente");
            entity.Property(e => e.FechaReserva)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreCliente).HasMaxLength(100);
            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");
            entity.Property(e => e.TelefonoCliente).HasMaxLength(20);
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Servicio).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.ServicioId)
                .HasConstraintName("FK__Reservas__servic__48CFD27E");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Reservas__usuari__47DBAE45");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Servicio__3213E83F890BF213");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreServicio).HasMaxLength(100);
            entity.Property(e => e.PrecioBase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TipoEvento).HasMaxLength(50);
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Servicios__usuar__3C69FB99");
        });

        modelBuilder.Entity<ServicioInventario>(entity =>
        {
            entity.HasKey(e => new { e.ServicioId, e.ItemId }).HasName("PK__Servicio__6A1A29F187F09820");

            entity.ToTable("ServicioInventario");

            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");

            entity.HasOne(d => d.Item).WithMany(p => p.ServicioInventarios)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServicioI__item___44FF419A");

            entity.HasOne(d => d.Servicio).WithMany(p => p.ServicioInventarios)
                .HasForeignKey(d => d.ServicioId)
                .HasConstraintName("FK__ServicioI__servi__440B1D61");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83FB8117998");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A1962EA4658").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Verificado).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
}