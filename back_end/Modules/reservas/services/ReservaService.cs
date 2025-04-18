using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Models;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.usuarios.Repositories;

namespace back_end.Modules.reservas.Services
{
    public interface IReservaService
    {
        Task<List<ReservaResponseDTO>> GetByCorreoAsync(string correo);
        Task<ReservaResponseDTO?> CreateAsync(string correo, ReservaCreateDTO dto);
        Task<ReservaResponseDTO?> UpdateAsync(string correo, Guid id, ReservaUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, Guid id);
    }

    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepo;
        private readonly IUsuarioRepository _usuarioRepo;

        public ReservaService(IReservaRepository reservaRepo, IUsuarioRepository usuarioRepo)
        {
            _reservaRepo = reservaRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<List<ReservaResponseDTO>> GetByCorreoAsync(string correo)
        {
            var reservas = await _reservaRepo.GetByCorreoUsuarioAsync(correo);
            return reservas.Select(MapToDTO).ToList();
        }

        public async Task<ReservaResponseDTO?> CreateAsync(string correo, ReservaCreateDTO dto)
        {
            var usuario = await _usuarioRepo.GetByCorreoAsync(correo);
            if (usuario == null) return null;

            var reserva = new Reserva
            {
                UsuarioId = usuario.Id,
                NombreEvento = dto.NombreEvento,
                FechaEvento = dto.FechaEvento,
                HoraEvento = dto.HoraEvento,
                TipoEvento = dto.TipoEvento,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado ?? "Pendiente",
                PrecioTotal = dto.PrecioTotal,
                ClienteId = dto.ClienteId ?? Guid.Empty
                // Otros campos se manejan mediante relaciones o no están en el modelo
            };

            var creada = await _reservaRepo.CreateAsync(reserva);
            return MapToDTO(creada);
        }

        public async Task<ReservaResponseDTO?> UpdateAsync(string correo, Guid id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return null;

            reserva.Estado = dto.Estado ?? reserva.Estado;
            // Actualizar otros campos según sean necesarios en el modelo

            var actualizada = await _reservaRepo.UpdateAsync(reserva);
            return MapToDTO(actualizada);
        }

        public async Task<bool> DeleteAsync(string correo, Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return false;

            return await _reservaRepo.DeleteAsync(reserva);
        }

        private ReservaResponseDTO MapToDTO(Reserva r) => new ReservaResponseDTO
        {
            Id = r.Id,
            NombreEvento = r.NombreEvento,
            FechaEvento = r.FechaEvento,
            HoraEvento = r.HoraEvento,
            TipoEvento = r.TipoEvento,
            Descripcion = r.Descripcion,
            Estado = r.Estado,
            PrecioTotal = r.PrecioTotal,
            // Obtener datos del cliente desde la relación
            NombreCliente = r.Cliente?.Nombre,
            CorreoCliente = r.Cliente?.CorreoElectronico,
            TelefonoCliente = r.Cliente?.Telefono,
            // Otros campos pueden requerir acceso a relaciones
            FechaReserva = DateTime.UtcNow // Mantener como está si no hay en el modelo
        };
    }
}
