using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface ITicketAdminService
    {
        Task<TicketResponse> CrearTicketAsync(CrearTicketRequest request, string usuarioAccion, string ip);
        Task<TicketResponse> ActualizarTicketAsync(Guid tckGuid, ActualizarTicketRequest request, string usuarioAccion, string ip);
        Task EliminarTicketAsync(Guid tckGuid, string usuarioAccion, string ip);
        Task<HorarioResponse> CrearHorarioAsync(CrearHorarioRequest request, string usuarioAccion, string ip);
    }
}
