using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Public
{
    public interface IReservaPublicService
    {
        Task<ReservaResponse> CrearAsync(CrearReservaRequest request, int cliId, string usuarioAccion, string ip);
        Task<ReservaResponse> ObtenerPorGuidAsync(Guid revGuid, int cliId);
        Task<DataPagedResult<ReservaResponse>> ListarPorClienteAsync(int cliId, int page, int limit);
    }
}
