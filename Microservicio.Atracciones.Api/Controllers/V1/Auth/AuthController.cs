using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Api.Services;
using Microservicio.Atracciones.Business.DTOs.Auth;
using Microservicio.Atracciones.Business.Interfaces.Auth;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.DTOs.Admin.Usuarios;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Atracciones.Api.Controllers.V1.Auth
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly TokenService _tokenService;
        private readonly IUsuarioAdminService _usuarioAdminService;

        public AuthController(IAuthService authService, TokenService tokenService, IUsuarioAdminService usuarioAdminService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _usuarioAdminService = usuarioAdminService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiItemResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var userDto = await _authService.ValidarCredencialesAsync(request);

            var (token, expiracion) = _tokenService.GenerarToken(new LoginDataModel
            {
                UsuId = userDto.UsuId,
                UsuGuid = userDto.UsuGuid,
                UsuLogin = userDto.Login,
                CliId = userDto.CliId, // E-05
                Roles = userDto.Roles.ToList()
            });

            var response = new ApiItemResponse<LoginResponse>(new LoginResponse
            {
                Token = token,
                Expiracion = expiracion,
                Login = userDto.Login,
                Roles = userDto.Roles
            });

            return Ok(response);
        }

        [HttpPost("registro")]
        [ProducesResponseType(typeof(ApiItemResponse<LoginResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> Registro([FromBody] LoginRequest request)
        {
            var crearRequest = new CrearUsuarioRequest
            {
                Login = request.Login,
                Password = request.Password,
                Roles = new List<string> { "CLIENTE" }
            };

            await _usuarioAdminService.CrearAsync(crearRequest, "publico", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0");

            var userDto = await _authService.ValidarCredencialesAsync(request);

            var (token, expiracion) = _tokenService.GenerarToken(new LoginDataModel
            {
                UsuId = userDto.UsuId,
                UsuGuid = userDto.UsuGuid,
                UsuLogin = userDto.Login,
                CliId = userDto.CliId,
                Roles = userDto.Roles.ToList()
            });

            var response = new ApiItemResponse<LoginResponse>(new LoginResponse
            {
                Token = token,
                Expiracion = expiracion,
                Login = userDto.Login,
                Roles = userDto.Roles
            }, 201);

            return StatusCode(201, response);
        }
    }
}
