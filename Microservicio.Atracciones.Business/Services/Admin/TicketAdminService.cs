using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Rules.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class TicketAdminService : ITicketAdminService
    {
        private readonly ITicketDataService _ticketService;
        private readonly IAtraccionDataService _atraccionService;
        private readonly TicketRules _rules;

        public TicketAdminService(ITicketDataService ticketService, IAtraccionDataService atraccionService, TicketRules rules)
        {
            _ticketService = ticketService;
            _atraccionService = atraccionService;
            _rules = rules;
        }

        public async Task<TicketResponse> CrearTicketAsync(CrearTicketRequest request, string usuarioAccion, string ip)
        {
            TicketAdminValidator.ValidarCrear(request);
            await _rules.ValidarAtraccionExisteAsync(request.AtGuid);

            var atraccion = await _atraccionService.ObtenerPorGuidAsync(request.AtGuid)!;

            var model = new TicketDataModel
            {
                AtId = atraccion!.AtId,
                TckTitulo = request.Titulo,
                TckPrecio = request.Precio,
                TckTipoParticipante = request.TipoParticipante,
                TckCapacidadMaxima = request.CapacidadMaxima,
                TckCuposDisponibles = request.CuposDisponibles,
                TckEstado = 'A',
                TckUsuarioIngreso = usuarioAccion,
                TckIpIngreso = ip
            };

            await _ticketService.CrearAsync(model);
            return TicketAdminMapper.ToResponse(model, atraccion.AtNombre);
        }

        public async Task<TicketResponse> ActualizarTicketAsync(Guid tckGuid, ActualizarTicketRequest request, string usuarioAccion, string ip)
        {
            var model = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);

            if (request.Titulo is not null) model.TckTitulo = request.Titulo;
            if (request.Precio is not null) model.TckPrecio = request.Precio.Value;
            if (request.Estado is not null) model.TckEstado = request.Estado.Value;
            if (request.CuposDisponibles is not null)
            {
                TicketRules.ValidarCupos(request.CuposDisponibles.Value, model.TckCapacidadMaxima);
                model.TckCuposDisponibles = request.CuposDisponibles.Value;
            }
            model.TckUsuarioMod = usuarioAccion;
            model.TckIpMod = ip;

            await _ticketService.ActualizarAsync(model);
            return TicketAdminMapper.ToResponse(model, string.Empty);
        }

        public async Task EliminarTicketAsync(Guid tckGuid, string usuarioAccion, string ip)
        {
            var model = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);
            await _ticketService.EliminarLogicoAsync(model.TckId, usuarioAccion, ip);
        }

        public async Task<TicketResponse> CrearHorarioAsync(CrearHorarioRequest request, string usuarioAccion, string ip)
        {
            TicketAdminValidator.ValidarCrearHorario(request);
            await _rules.ValidarTicketExisteAsync(request.TckGuid);

            var ticket = await _ticketService.ObtenerPorGuidAsync(request.TckGuid)!;

            var horario = new HorarioDataModel
            {
                TckId = ticket!.TckId,
                HorFecha = request.Fecha,
                HorHoraInicio = request.HoraInicio,
                HorHoraFin = request.HoraFin,
                HorCuposDisponibles = request.CuposDisponibles,
                HorEstado = 'A',
                HorUsuarioIngreso = usuarioAccion,
                HorIpIngreso = ip
            };

            await _ticketService.CrearHorarioAsync(horario);
            return TicketAdminMapper.ToResponse(ticket, string.Empty);
        }
    }
}
