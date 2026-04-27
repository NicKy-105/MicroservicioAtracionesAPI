using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Public
{
    public interface IAtraccionPublicService
    {
        Task<DataPagedResult<AtraccionListadoResponse>> ListarAsync(AtraccionFiltroRequest request, string baseUrl);
        Task<AtraccionDetalleResponse> ObtenerPorGuidAsync(Guid atGuid, string baseUrl);
        Task<FiltrosAtraccionResponse> ObtenerFiltrosAsync();
    }

}
