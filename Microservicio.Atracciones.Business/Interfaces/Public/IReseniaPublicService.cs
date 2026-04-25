using Microservicio.Atracciones.Business.DTOs.Public.Resenias;

namespace Microservicio.Atracciones.Business.Interfaces.Public
{
    public interface IReseniaPublicService
    {
        Task<ReseniaResponse> CrearAsync(CrearReseniaRequest request, int cliId, string usuarioAccion, string ip);
        Task<IReadOnlyList<ReseniaResponse>> ListarPorAtraccionAsync(Guid atGuid);
    }
}
