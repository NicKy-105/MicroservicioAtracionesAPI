using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class TicketAdminMapper
    {
        public static TicketResponse ToResponse(TicketDataModel model, string atraccionNombre)
            => new()
            {
                TckGuid = model.TckGuid.ToString(),
                AtraccionNombre = atraccionNombre,
                Titulo = model.TckTitulo,
                Precio = model.TckPrecio,
                TipoParticipante = model.TckTipoParticipante,
                CapacidadMaxima = model.TckCapacidadMaxima,
                CuposDisponibles = model.TckCuposDisponibles,
                Estado = model.TckEstado
            };
    }
}
