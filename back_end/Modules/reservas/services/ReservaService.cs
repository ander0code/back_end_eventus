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
        Task<ReservaResponseDTO?> UpdateAsync(string correo, int id, ReservaUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, int id);
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
                NombreCliente = dto.NombreCliente,
                CorreoCliente = dto.CorreoCliente,
                TelefonoCliente = dto.TelefonoCliente,
                ServicioId = dto.ServicioId,
                FechaEvento = dto.FechaEvento,
                Estado = dto.Estado ?? "Pendiente",
                Observaciones = dto.Observaciones,
                FechaReserva = DateTime.UtcNow
            };

            var creada = await _reservaRepo.CreateAsync(reserva);
            return MapToDTO(creada);
        }

        public async Task<ReservaResponseDTO?> UpdateAsync(string correo, int id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return null;

            reserva.Estado = dto.Estado ?? reserva.Estado;
            reserva.Observaciones = dto.Observaciones ?? reserva.Observaciones;

            var actualizada = await _reservaRepo.UpdateAsync(reserva);
            return MapToDTO(actualizada);
        }

        public async Task<bool> DeleteAsync(string correo, int id)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return false;

            return await _reservaRepo.DeleteAsync(reserva);
        }

        private ReservaResponseDTO MapToDTO(Reserva r) => new ReservaResponseDTO
        {
            Id = r.Id,
            NombreCliente = r.NombreCliente,
            CorreoCliente = r.CorreoCliente,
            TelefonoCliente = r.TelefonoCliente,
            ServicioId = r.ServicioId,
            FechaEvento = r.FechaEvento,
            Estado = r.Estado,
            FechaReserva = r.FechaReserva,
            Observaciones = r.Observaciones
        };
    }
}
