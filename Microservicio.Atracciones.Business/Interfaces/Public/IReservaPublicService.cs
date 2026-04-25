using Microservicio.Atracciones.Business.DTOs.Public.Reservas;

namespace Microservicio.Atracciones.Business.Interfaces.Public
{
    public interface IReservaPublicService
    {
        Task<ReservaResponse> CrearAsync(CrearReservaRequest request, int cliId, string usuarioAccion, string ip);
        Task<ReservaResponse> ObtenerPorGuidAsync(Guid revGuid, int cliId);
    }
}
