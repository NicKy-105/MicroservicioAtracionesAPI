using Microservicio.Atracciones.Business.DTOs.Admin.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class ReservaAdminMapper
    {
        public static ReservaAdminResponse ToResponse(ReservaDataModel model, string clienteNombre, string atraccionNombre)
            => new()
            {
                RevGuid = model.RevGuid.ToString(),
                RevCodigo = model.RevCodigo,
                ClienteNombre = clienteNombre,
                AtraccionNombre = atraccionNombre,
                HorFecha = string.Empty,   // se completa en el service con el horario
                HorHoraInicio = string.Empty,
                RevTotal = model.RevTotal,
                RevEstado = model.RevEstado,
                FechaReserva = model.RevFechaReservaUtc,
                Detalle = model.Detalle.Select(d => new ReservaDetalleAdminResponse
                {
                    TipoParticipante = d.TckTipoParticipante,
                    Cantidad = d.RdetCantidad,
                    PrecioUnit = d.RdetPrecioUnit,
                    Subtotal = d.RdetSubtotal
                }).ToList()
            };
    }
}
