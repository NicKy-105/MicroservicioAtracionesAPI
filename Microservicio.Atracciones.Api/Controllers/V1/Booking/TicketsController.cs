using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/tickets")]
    [Authorize(Policy = "AdminOOperador")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketAdminService _service;

        public TicketsController(ITicketAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<TicketResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Crear([FromBody] CrearTicketRequest request)
        {
            var ticket = await _service.CrearTicketAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<TicketResponse>(ticket, 201));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<TicketResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarTicketRequest request)
        {
            var ticket = await _service.ActualizarTicketAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<TicketResponse>(ticket));
        }

        [HttpPost("horarios")]
        [ProducesResponseType(typeof(ApiItemResponse<HorarioResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> CrearHorario([FromBody] CrearHorarioRequest request)
        {
            var horario = await _service.CrearHorarioAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<HorarioResponse>(horario, 201));
        }
    }
}
