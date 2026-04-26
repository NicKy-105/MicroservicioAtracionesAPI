using Asp.Versioning;
using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Mappers.Public;
using Microservicio.Atracciones.Api.Models.Common;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/reservas")]
    [Authorize]   // requiere token de cliente
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaPublicService _service;

        public ReservasController(IReservaPublicService service)
            => _service = service;

        private int CliIdActual => int.Parse(User.FindFirstValue("cli_id") ?? "0");
        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiListResponse<ReservaResponse>), 200)]
        public async Task<IActionResult> ListarMisReservas([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var resultado = await _service.ListarPorClienteAsync(CliIdActual, page, limit);
            var response = new ApiListResponse<ReservaResponse>(resultado.Items, resultado.TotalFiltrado, page, limit);
            return Ok(response);
        }

        // ----------------------------------------------------------------
        //  POST /api/v1/reservas
        //  Crea reserva con cabecera + detalle en una sola transacción.
        //  Descuenta cupos en HORARIO. IVA 15%.
        // ----------------------------------------------------------------
        [HttpPost]
        [EndpointName(EndpointNames.CrearReserva)]
        [ProducesResponseType(typeof(ApiItemResponse<ReservaResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Crear([FromBody] CrearReservaRequest request)
        {
            var reserva = await _service.CrearAsync(request, CliIdActual, UsuarioAccion, IpActual);
            var response = ReservasApiMapper.ToResponse(reserva, statusCode: 201);
            return StatusCode(201, response);
        }

        // ----------------------------------------------------------------
        //  GET /api/v1/reservas/{guid}
        //  El cliente solo puede ver sus propias reservas.
        // ----------------------------------------------------------------
        [HttpGet("{guid:guid}")]
        [EndpointName(EndpointNames.ObtenerReserva)]
        [ProducesResponseType(typeof(ApiItemResponse<ReservaResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 403)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerPorGuid(Guid guid)
        {
            var reserva = await _service.ObtenerPorGuidAsync(guid, CliIdActual);
            var response = ReservasApiMapper.ToResponse(reserva);
            return Ok(response);
        }
    }
}
