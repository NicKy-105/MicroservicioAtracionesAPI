using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.Business.Mappers.Public;
using Microservicio.Atracciones.Business.Rules.Public;
using Microservicio.Atracciones.Business.Validators.Public;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Services.Public
{
    public class ReservaPublicService : IReservaPublicService
    {
        private readonly IReservaDataService _reservaService;
        private readonly ITicketDataService _ticketService;
        private readonly IAtraccionDataService _atraccionService;  // #7: para obtener nombre real
        private readonly ReservaRules _rules;

        public ReservaPublicService(
            IReservaDataService reservaService,
            ITicketDataService ticketService,
            IAtraccionDataService atraccionService,
            ReservaRules rules)
        {
            _reservaService = reservaService;
            _ticketService = ticketService;
            _atraccionService = atraccionService;
            _rules = rules;
        }

        public async Task<ReservaResponse> CrearAsync(
            CrearReservaRequest request, int cliId, string usuarioAccion, string ip)
        {
            ReservaPublicValidator.Validar(request);

            // 1. Validar y obtener tickets con precios actuales
            var lineas = request.Lineas.Select(l => (l.TckGuid, l.Cantidad)).ToList();
            var ticketsValidos = await _rules.ValidarYObtenerTicketsAsync(
                request.HorGuid, lineas);

            // 2. Verificar cupos totales
            var totalPersonas = ticketsValidos.Sum(t => t.Cantidad);
            await _rules.ValidarDisponibilidadAsync(request.HorGuid, totalPersonas);

            // 3. Obtener horario
            var horario = await _ticketService.ObtenerHorarioPorGuidAsync(request.HorGuid)
                ?? throw new NotFoundException("Horario", request.HorGuid);

            // 4. Calcular totales (IVA 15%)
            var (subtotal, iva, total) = ReservaRules.CalcularTotales(ticketsValidos);

            // 5. Construir modelo de reserva con líneas
            var reservaModel = new ReservaDataModel
            {
                CliId = cliId,
                HorId = horario.HorId,
                RevCodigo = ReservaRules.GenerarCodigo(),
                RevSubtotal = subtotal,
                RevValorIva = iva,
                RevTotal = total,
                RevOrigenCanal = request.OrigenCanal,
                RevUsuarioIngreso = usuarioAccion,
                RevIpIngreso = ip,
                Detalle = ticketsValidos.Select(t => new ReservaDetalleDataModel
                {
                    TckId = t.Ticket.TckId,
                    RdetCantidad = t.Cantidad,
                    RdetPrecioUnit = t.Ticket.TckPrecio,   // precio congelado
                    RdetSubtotal = t.Ticket.TckPrecio * t.Cantidad,
                    TckTipoParticipante = t.Ticket.TckTipoParticipante,
                    RdetUsuarioIngreso = usuarioAccion,
                    RdetIpIngreso = ip
                }).ToList()
            };

            await _reservaService.CrearAsync(reservaModel);

            // 6. Descontar cupos en HORARIO
            var horarioActualizado = new HorarioDataModel
            {
                HorId = horario.HorId,
                HorGuid = horario.HorGuid,
                TckId = horario.TckId,
                HorFecha = horario.HorFecha,
                HorHoraInicio = horario.HorHoraInicio,
                HorHoraFin = horario.HorHoraFin,
                HorCuposDisponibles = horario.HorCuposDisponibles - totalPersonas,
                HorEstado = horario.HorEstado,
                HorUsuarioMod = usuarioAccion,
                HorIpMod = ip
            };
            await _ticketService.ActualizarHorarioAsync(horarioActualizado);

            // 7. Obtener nombre real de la atracción
            var ticketModel  = await _ticketService.ObtenerPorIdAsync(horario.TckId);
            var atraccion    = ticketModel is not null
                ? await _atraccionService.ObtenerPorIdAsync(ticketModel.AtId)
                : null;
            var atraccionNombre = atraccion?.AtNombre ?? "Atracción";

            return ReservaPublicMapper.ToResponse(reservaModel, horario, atraccionNombre);
        }

        public async Task<ReservaResponse> ObtenerPorGuidAsync(Guid revGuid, int cliId)
        {
            var reserva = await _reservaService.ObtenerPorGuidAsync(revGuid)
                ?? throw new NotFoundException("Reserva", revGuid);

            if (reserva.CliId != cliId)
                throw new ForbiddenBusinessException("No tienes acceso a esta reserva.");

            // #1: resuelve el horario real usando el HorId que ya viene en el modelo
            var horario = await _ticketService.ObtenerHorarioPorIdAsync(reserva.HorId)
                ?? new HorarioDataModel();

            // #7: nombre real de la atracción
            var ticketModel = horario.TckId > 0
                ? await _ticketService.ObtenerPorIdAsync(horario.TckId)
                : null;
            var atraccion   = ticketModel is not null
                ? await _atraccionService.ObtenerPorIdAsync(ticketModel.AtId)
                : null;
            var atraccionNombre = atraccion?.AtNombre ?? "Atracción";

            return ReservaPublicMapper.ToResponse(reserva, horario, atraccionNombre);
        }

        public async Task<DataPagedResult<ReservaResponse>> ListarPorClienteAsync(int cliId, int page, int limit)
        {
            var paged = await _reservaService.ListarPorClienteAsync(cliId, page, limit);
            var list = new List<ReservaResponse>();

            foreach(var reserva in paged.Items)
            {
                var horario = await _ticketService.ObtenerHorarioPorIdAsync(reserva.HorId) ?? new HorarioDataModel();
                var ticketModel = horario.TckId > 0 ? await _ticketService.ObtenerPorIdAsync(horario.TckId) : null;
                var atraccion = ticketModel is not null ? await _atraccionService.ObtenerPorIdAsync(ticketModel.AtId) : null;
                var atraccionNombre = atraccion?.AtNombre ?? "Atracción";
                list.Add(ReservaPublicMapper.ToResponse(reserva, horario, atraccionNombre));
            }
            return new DataPagedResult<ReservaResponse>(list, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }
    }
}
