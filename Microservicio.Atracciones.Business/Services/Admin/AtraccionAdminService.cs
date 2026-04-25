using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.Business.Mappers.Admin;
using Microservicio.Atracciones.Business.Rules.Admin;
using Microservicio.Atracciones.Business.Validators.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;


namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class AtraccionAdminService : IAtraccionAdminService
    {
        private readonly IAtraccionDataService _atraccionService;
        private readonly IDestinoDataService _destinoService;
        private readonly AtraccionRules _rules;

        public AtraccionAdminService(IAtraccionDataService atraccionService, IDestinoDataService destinoService, AtraccionRules rules)
        {
            _atraccionService = atraccionService;
            _destinoService = destinoService;
            _rules = rules;
        }

        public async Task<AtraccionAdminResponse> CrearAsync(CrearAtraccionRequest request, string usuarioAccion, string ip)
        {
            AtraccionAdminValidator.ValidarCrear(request);
            await _rules.ValidarDestinoExisteAsync(request.DesGuid);

            var destinoModel = await _destinoService.ObtenerPorGuidAsync(request.DesGuid);
            var destino = destinoModel!.DesId;

            var model = new AtraccionDataModel
            {
                DesId = destino,
                AtNumEstablecimiento = request.NumEstablecimiento,
                AtNombre = request.Nombre,
                AtDescripcion = request.Descripcion,
                AtDireccion = request.Direccion,
                AtDuracionMinutos = request.DuracionMinutos,
                AtPuntoEncuentro = request.PuntoEncuentro,
                AtPrecioReferencia = request.PrecioReferencia,
                AtIncluyeAcompaniante = request.IncluyeAcompaniante,
                AtIncluyeTransporte = request.IncluyeTransporte,
                AtEstado = 'A',
                AtUsuarioIngreso = usuarioAccion,
                AtIpIngreso = ip
            };

            await _atraccionService.CrearAsync(model);
            return AtraccionAdminMapper.ToResponse(model);
        }

        public async Task<AtraccionAdminResponse> ActualizarAsync(Guid atGuid, ActualizarAtraccionRequest request, string usuarioAccion, string ip)
        {
            var model = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            if (request.Nombre is not null) model.AtNombre = request.Nombre;
            if (request.Descripcion is not null) model.AtDescripcion = request.Descripcion;
            if (request.Direccion is not null) model.AtDireccion = request.Direccion;
            if (request.DuracionMinutos is not null) model.AtDuracionMinutos = request.DuracionMinutos;
            if (request.PuntoEncuentro is not null) model.AtPuntoEncuentro = request.PuntoEncuentro;
            if (request.PrecioReferencia is not null) model.AtPrecioReferencia = request.PrecioReferencia;
            if (request.IncluyeAcompaniante is not null) model.AtIncluyeAcompaniante = request.IncluyeAcompaniante.Value;
            if (request.IncluyeTransporte is not null) model.AtIncluyeTransporte = request.IncluyeTransporte.Value;
            if (request.Disponible is not null) model.AtDisponible = request.Disponible.Value;
            model.AtUsuarioMod = usuarioAccion;
            model.AtIpMod = ip;

            await _atraccionService.ActualizarAsync(model);
            return AtraccionAdminMapper.ToResponse(model);
        }

        public async Task EliminarAsync(Guid atGuid, string usuarioAccion, string ip)
        {
            _ = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);
            await _atraccionService.EliminarLogicoAsync(0, usuarioAccion, ip);  // AtId resuelto internamente
        }

        public async Task<AtraccionAdminResponse> ObtenerPorGuidAsync(Guid atGuid)
        {
            var model = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);
            return AtraccionAdminMapper.ToResponse(model);
        }

        public async Task<DataPagedResult<AtraccionAdminResponse>> ListarAsync(AtraccionAdminFiltroRequest filtro)
        {
            var dmFiltro = new AtraccionFiltroDataModel
            {
                Ciudad = filtro.Ciudad,
                Page = filtro.Page,
                Limit = filtro.Limit
            };

            var paged = await _atraccionService.ListarConFiltrosAsync(dmFiltro);
            var items = paged.Items.Select(m => AtraccionAdminMapper.ToResponse(m)).ToList();
            return new DataPagedResult<AtraccionAdminResponse>(items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }
    }
}
