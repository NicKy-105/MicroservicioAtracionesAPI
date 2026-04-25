using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class AtraccionAdminMapper
    {
        public static AtraccionAdminResponse ToResponse(AtraccionDataModel model)
            => new()
            {
                AtGuid = model.AtGuid.ToString(),
                Nombre = model.AtNombre,
                Ciudad = model.DesNombre,
                Pais = model.DesPais,
                Descripcion = model.AtDescripcion,
                DuracionMinutos = model.AtDuracionMinutos,
                PrecioReferencia = model.AtPrecioReferencia,
                Disponible = model.AtDisponible,
                Estado = model.AtEstado,
                TotalResenias = model.AtTotalResenias,
                FechaIngreso = model.AtFechaIngreso
            };
    }
}
