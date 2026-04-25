using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Api.Services;
using Microservicio.Atracciones.Business.DTOs.Auth;
using Microservicio.Atracciones.Business.Interfaces.Auth;
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

        public AuthController(IAuthService authService, TokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
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
    }
}
