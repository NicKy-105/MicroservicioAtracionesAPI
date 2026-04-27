using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Interfaces.Auth;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class ClienteAdminService : IClienteAdminService
    {
        private readonly IClienteDataService _clienteService;
        private readonly IUsuarioDataService _usuarioService;
        private readonly IPasswordHasher _passwordHasher;

        public ClienteAdminService(
            IClienteDataService clienteService,
            IUsuarioDataService usuarioService,
            IPasswordHasher passwordHasher)
        {
            _clienteService = clienteService;
            _usuarioService = usuarioService;
            _passwordHasher = passwordHasher;
        }

        public async Task<ClienteResponse> CrearAsync(CrearClienteRequest request, string usuarioAccion, string ip)
        {
            ClienteAdminValidator.ValidarCrear(request);

            if (await _clienteService.ExisteIdentificacionAsync(request.NumeroIdentificacion))
                throw new ConflictException($"Ya existe un cliente con la identificación '{request.NumeroIdentificacion}'.");

            if (await _usuarioService.ExisteLoginAsync(request.Login))
                throw new ConflictException($"El login '{request.Login}' ya está en uso.");

            // 1. Crear usuario de acceso
            var usuarioModel = new UsuarioDataModel
            {
                UsuLogin = request.Login,
                UsuPasswordHash = _passwordHasher.Hashear(request.Password),
                UsuEstado = 'A',
                UsuUsuarioRegistro = usuarioAccion,
                UsuIpRegistro = ip,
                Roles = new List<RolDataModel>
                {
                    new() { RolDescripcion = "CLIENTE" }
                }
            };
            try
            {
                await _usuarioService.CrearAsync(usuarioModel);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Roles no encontrados"))
            {
                throw new ConflictException(ex.Message);
            }

            // 2. Crear cliente vinculado al usuario
            var clienteModel = new ClienteDataModel
            {
                UsuId = usuarioModel.UsuId,
                CliTipoIdentificacion = request.TipoIdentificacion,
                CliNumeroIdentificacion = request.NumeroIdentificacion,
                CliNombres = request.Nombres,
                CliApellidos = request.Apellidos,
                CliRazonSocial = request.RazonSocial,
                CliCorreo = request.Correo,
                CliTelefono = request.Telefono,
                CliDireccion = request.Direccion,
                CliEstado = 'A',
                CliUsuarioIngreso = usuarioAccion,
                CliIpIngreso = ip
            };
            await _clienteService.CrearAsync(clienteModel);

            return ClienteAdminMapper.ToResponse(clienteModel);
        }

        public async Task<ClienteResponse> ActualizarAsync(Guid cliGuid, ActualizarClienteRequest request, string usuarioAccion, string ip)
        {
            var model = await _clienteService.ObtenerPorGuidAsync(cliGuid)
                ?? throw new NotFoundException("Cliente", cliGuid);

            if (request.Correo is not null) model.CliCorreo = request.Correo;
            if (request.Telefono is not null) model.CliTelefono = request.Telefono;
            if (request.Direccion is not null) model.CliDireccion = request.Direccion;
            if (request.Nombres is not null) model.CliNombres = request.Nombres;
            if (request.Apellidos is not null) model.CliApellidos = request.Apellidos;
            if (request.RazonSocial is not null) model.CliRazonSocial = request.RazonSocial;

            await _clienteService.ActualizarAsync(model);
            return ClienteAdminMapper.ToResponse(model);
        }

        public async Task EliminarAsync(Guid cliGuid, string usuarioAccion, string ip)
        {
            var model = await _clienteService.ObtenerPorGuidAsync(cliGuid)
                ?? throw new NotFoundException("Cliente", cliGuid);
            await _clienteService.EliminarLogicoAsync(model.CliId, usuarioAccion, ip);
        }

        public async Task<ClienteResponse> ObtenerPorGuidAsync(Guid cliGuid)
        {
            var model = await _clienteService.ObtenerPorGuidAsync(cliGuid)
                ?? throw new NotFoundException("Cliente", cliGuid);
            return ClienteAdminMapper.ToResponse(model);
        }

        public async Task<DataPagedResult<ClienteResponse>> ListarAsync(ClienteFiltroRequest filtro)
        {
            var todos = await _clienteService.ListarAsync();
            var filtrados = todos
                .Where(c => filtro.Estado is null || c.CliEstado == filtro.Estado)
                .ToList();

            var items = filtrados
                .Skip((filtro.Page - 1) * filtro.Limit)
                .Take(filtro.Limit)
                .Select(c => ClienteAdminMapper.ToResponse(c))
                .ToList();

            return new DataPagedResult<ClienteResponse>(items, filtrados.Count, todos.Count, filtro.Page, filtro.Limit);
        }
    }
}
