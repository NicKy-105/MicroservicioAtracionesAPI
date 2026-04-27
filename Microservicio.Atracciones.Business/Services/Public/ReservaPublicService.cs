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
        private readonly IUsuarioDataService _usuarioService;
        private readonly IClienteDataService _clienteService;
        private readonly ReservaRules _rules;
        private readonly IUnitOfWork _unitOfWork;

        public ReservaPublicService(
            IReservaDataService reservaService,
            ITicketDataService ticketService,
            IAtraccionDataService atraccionService,
            IUsuarioDataService usuarioService,
            IClienteDataService clienteService,
            ReservaRules rules,
            IUnitOfWork unitOfWork)
        {
            _reservaService = reservaService;
            _ticketService = ticketService;
            _atraccionService = atraccionService;
            _usuarioService = usuarioService;
            _clienteService = clienteService;
            _rules = rules;
            _unitOfWork = unitOfWork;
        }

        public async Task<ReservaResponse> CrearAsync(
            CrearReservaRequest request, Guid usuGuid, string usuarioAccion, string ip)
        {
            ReservaPublicValidator.Validar(request);
            var cliId = await ObtenerCliIdAsync(usuGuid);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Validar y obtener tickets con precios actuales
                var lineas = request.Lineas.Select(l => (l.TckGuid, l.Cantidad)).ToList();
                var ticketsValidos = await _rules.ValidarYObtenerTicketsAsync(
                    request.HorGuid, request.AtGuid, lineas);

                // 2. Verificar cupos totales
                var totalPersonas = ticketsValidos.Sum(t => t.Cantidad);
                await _rules.ValidarDisponibilidadAsync(request.HorGuid, totalPersonas);

                // 3. Obtener horario
                var horario = await _ticketService.ObtenerHorarioPorGuidAsync(request.HorGuid)
                    ?? throw new NotFoundException("Horario", request.HorGuid);

                // 3.1 Obtener nombre real de la atracción y validar AtGuid
                var ticketModel  = await _ticketService.ObtenerPorIdAsync(horario.TckId);
                var atraccion    = ticketModel is not null
                    ? await _atraccionService.ObtenerPorIdAsync(ticketModel.AtId)
                    : null;
                    
                if (atraccion == null)
                    throw new ConflictException("La atracción del horario no fue encontrada.");
                    
                if (atraccion.AtGuid != request.AtGuid)
                    throw new ConflictException("El GUID de la atracción no coincide con el horario seleccionado.");
                    
                var atraccionNombre = atraccion.AtNombre;

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

                await transaction.CommitAsync();
                
                return ReservaPublicMapper.ToResponse(reservaModel, horario, atraccionNombre);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ReservaResponse> ObtenerPorGuidAsync(Guid revGuid, Guid usuGuid)
        {
            var cliId = await ObtenerCliIdAsync(usuGuid);
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

        public async Task<DataPagedResult<ReservaResponse>> ListarPorClienteAsync(Guid usuGuid, int page, int limit)
        {
            var cliId = await ObtenerCliIdAsync(usuGuid);
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

        private async Task<int> ObtenerCliIdAsync(Guid usuGuid)
        {
            var usuario = await _usuarioService.ObtenerPorGuidAsync(usuGuid)
                ?? throw new UnauthorizedBusinessException("El token no corresponde a un usuario activo.");

            var cliente = await _clienteService.ObtenerPorUsuarioIdAsync(usuario.UsuId)
                ?? throw new NotFoundException("Cliente asociado al usuario", usuGuid);

            return cliente.CliId;
        }

    }
}
