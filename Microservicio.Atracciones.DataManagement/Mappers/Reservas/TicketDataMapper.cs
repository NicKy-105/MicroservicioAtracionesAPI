using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.DataManagement.Mappers.Reservas
{
    public static class TicketDataMapper
    {
        public static TicketDataModel? ToDataModel(TicketEntity? entity)
        {
            if (entity is null) return null;

            return new TicketDataModel
            {
                TckId = entity.TckId,
                TckGuid = entity.TckGuid,
                AtId = entity.AtId,
                TckTitulo = entity.TckTitulo,
                TckPrecio = entity.TckPrecio,
                TckTipoParticipante = entity.TckTipoParticipante,
                TckCapacidadMaxima = entity.TckCapacidadMaxima,
                TckCuposDisponibles = entity.TckCuposDisponibles,
                TckEstado = entity.TckEstado,
                TckFechaIngreso = entity.TckFechaIngreso,
                TckUsuarioIngreso = entity.TckUsuarioIngreso,
                TckIpIngreso = entity.TckIpIngreso,
                TckFechaMod = entity.TckFechaMod,
                TckUsuarioMod = entity.TckUsuarioMod,
                TckIpMod = entity.TckIpMod,
                TckFechaEliminacion = entity.TckFechaEliminacion,
                TckUsuarioEliminacion = entity.TckUsuarioEliminacion,
                TckIpEliminacion = entity.TckIpEliminacion
            };
        }

        public static TicketEntity ToNewEntity(TicketDataModel model)
        {
            return new TicketEntity
            {
                TckGuid = Guid.NewGuid(),
                AtId = model.AtId,
                TckTitulo = model.TckTitulo,
                TckPrecio = model.TckPrecio,
                TckTipoParticipante = model.TckTipoParticipante,
                TckCapacidadMaxima = model.TckCapacidadMaxima,
                TckCuposDisponibles = model.TckCuposDisponibles,
                TckEstado = 'A',
                TckFechaIngreso = DateTime.UtcNow,
                TckUsuarioIngreso = model.TckUsuarioIngreso,
                TckIpIngreso = model.TckIpIngreso
            };
        }

        public static void ApplyToEntity(TicketDataModel model, TicketEntity entity)
        {
            entity.TckTitulo = model.TckTitulo;
            entity.TckPrecio = model.TckPrecio;
            entity.TckTipoParticipante = model.TckTipoParticipante;
            entity.TckCapacidadMaxima = model.TckCapacidadMaxima;
            entity.TckCuposDisponibles = model.TckCuposDisponibles;
            entity.TckEstado = model.TckEstado;
            entity.TckFechaMod = DateTime.UtcNow;
            entity.TckUsuarioMod = model.TckUsuarioMod;
            entity.TckIpMod = model.TckIpMod;
        }

        public static HorarioDataModel? ToHorarioDataModel(HorarioEntity? entity)
        {
            if (entity is null) return null;

            return new HorarioDataModel
            {
                HorId = entity.HorId,
                HorGuid = entity.HorGuid,
                TckId = entity.TckId,
                HorFecha = entity.HorFecha,
                HorHoraInicio = entity.HorHoraInicio,
                HorHoraFin = entity.HorHoraFin,
                HorCuposDisponibles = entity.HorCuposDisponibles,
                HorEstado = entity.HorEstado,
                HorFechaIngreso = entity.HorFechaIngreso,
                HorUsuarioIngreso = entity.HorUsuarioIngreso,
                HorIpIngreso = entity.HorIpIngreso,
                HorFechaMod = entity.HorFechaMod,
                HorUsuarioMod = entity.HorUsuarioMod,
                HorIpMod = entity.HorIpMod,
                HorFechaEliminacion = entity.HorFechaEliminacion,
                HorUsuarioEliminacion = entity.HorUsuarioEliminacion,
                HorIpEliminacion = entity.HorIpEliminacion
            };
        }

        public static HorarioEntity ToNewHorarioEntity(HorarioDataModel model)
        {
            return new HorarioEntity
            {
                HorGuid = Guid.NewGuid(),
                TckId = model.TckId,
                HorFecha = model.HorFecha,
                HorHoraInicio = model.HorHoraInicio,
                HorHoraFin = model.HorHoraFin,
                HorCuposDisponibles = model.HorCuposDisponibles,
                HorEstado = 'A',
                HorFechaIngreso = DateTime.UtcNow,
                HorUsuarioIngreso = model.HorUsuarioIngreso,
                HorIpIngreso = model.HorIpIngreso
            };
        }
    }
}
