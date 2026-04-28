using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Catalogos;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    public class CatalogosAdminController : ControllerBase
    {
        private readonly ICategoriaDataService _categoriaService;
        private readonly IIdiomaDataService _idiomaService;
        private readonly IIncluyeDataService _incluyeService;

        public CatalogosAdminController(
            ICategoriaDataService categoriaService,
            IIdiomaDataService idiomaService,
            IIncluyeDataService incluyeService)
        {
            _categoriaService = categoriaService;
            _idiomaService = idiomaService;
            _incluyeService = incluyeService;
        }

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet("categorias")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<CategoriaResponse>>), 200)]
        public async Task<IActionResult> ListarCategorias()
        {
            var data = await _categoriaService.ListarActivasAsync();
            var response = data.Select(c => new CategoriaResponse
            {
                CatGuid = c.CatGuid.ToString(),
                Nombre = c.CatNombre,
                ParentGuid = c.ParentGuid?.ToString(),
                ParentNombre = c.ParentNombre
            }).ToList();
            
            return Ok(new ApiItemResponse<IReadOnlyList<CategoriaResponse>>(response));
        }

        [HttpPost("categorias")]
        [ProducesResponseType(typeof(ApiItemResponse<CategoriaResponse>), 201)]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaRequest request)
        {
            var model = new CategoriaDataModel
            {
                CatNombre = request.Nombre.Trim(),
                ParentGuid = request.ParentGuid
            };

            await _categoriaService.CrearAsync(model, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<CategoriaResponse>(ToCategoriaResponse(model), 201));
        }

        [HttpPut("categorias/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<CategoriaResponse>), 200)]
        public async Task<IActionResult> ActualizarCategoria(Guid guid, [FromBody] ActualizarCategoriaRequest request)
        {
            var model = await _categoriaService.ObtenerPorGuidAsync(guid)
                ?? throw new NotFoundException("Categoría", guid);

            model.CatNombre = request.Nombre.Trim();
            model.ParentGuid = request.ParentGuid;
            await _categoriaService.ActualizarAsync(model, UsuarioAccion, IpActual);

            var actualizado = await _categoriaService.ObtenerPorGuidAsync(guid) ?? model;
            return Ok(new ApiItemResponse<CategoriaResponse>(ToCategoriaResponse(actualizado)));
        }

        [HttpDelete("categorias/{guid:guid}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> EliminarCategoria(Guid guid)
        {
            var model = await _categoriaService.ObtenerPorGuidAsync(guid)
                ?? throw new NotFoundException("Categoría", guid);

            await _categoriaService.EliminarLogicoAsync(model.CatId, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpGet("idiomas")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<IdiomaResponse>>), 200)]
        public async Task<IActionResult> ListarIdiomas()
        {
            var data = await _idiomaService.ListarActivosAsync();
            var response = data.Select(i => new IdiomaResponse
            {
                IdGuid = i.IdGuid.ToString(),
                Descripcion = i.IdDescripcion
            }).ToList();

            return Ok(new ApiItemResponse<IReadOnlyList<IdiomaResponse>>(response));
        }

        [HttpPost("idiomas")]
        [ProducesResponseType(typeof(ApiItemResponse<IdiomaResponse>), 201)]
        public async Task<IActionResult> CrearIdioma([FromBody] CrearIdiomaRequest request)
        {
            var model = new IdiomaDataModel { IdDescripcion = request.Descripcion.Trim() };
            await _idiomaService.CrearAsync(model, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<IdiomaResponse>(ToIdiomaResponse(model), 201));
        }

        [HttpPut("idiomas/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<IdiomaResponse>), 200)]
        public async Task<IActionResult> ActualizarIdioma(Guid guid, [FromBody] ActualizarIdiomaRequest request)
        {
            var model = await _idiomaService.ObtenerPorGuidAsync(guid)
                ?? throw new NotFoundException("Idioma", guid);

            model.IdDescripcion = request.Descripcion.Trim();
            await _idiomaService.ActualizarAsync(model, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<IdiomaResponse>(ToIdiomaResponse(model)));
        }

        [HttpDelete("idiomas/{guid:guid}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> EliminarIdioma(Guid guid)
        {
            var model = await _idiomaService.ObtenerPorGuidAsync(guid)
                ?? throw new NotFoundException("Idioma", guid);

            await _idiomaService.EliminarLogicoAsync(model.IdId, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpGet("incluye")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<IncluyeResponse>>), 200)]
        public async Task<IActionResult> ListarIncluye()
        {
            var data = await _incluyeService.ListarActivosAsync();
            var response = data.Select(i => new IncluyeResponse
            {
                IncluyeGuid = i.IncGuid.ToString(),
                Descripcion = i.IncDescripcion
            }).ToList();

            return Ok(new ApiItemResponse<IReadOnlyList<IncluyeResponse>>(response));
        }

        [HttpPost("incluye")]
        [ProducesResponseType(typeof(ApiItemResponse<IncluyeResponse>), 201)]
        public async Task<IActionResult> CrearIncluye([FromBody] CrearIncluyeRequest request)
        {
            var model = new IncluyeDataModel { IncDescripcion = request.Descripcion.Trim() };
            await _incluyeService.CrearAsync(model);
            return StatusCode(201, new ApiItemResponse<IncluyeResponse>(ToIncluyeResponse(model), 201));
        }

        [HttpPut("incluye/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<IncluyeResponse>), 200)]
        public async Task<IActionResult> ActualizarIncluye(Guid guid, [FromBody] ActualizarIncluyeRequest request)
        {
            var model = await _incluyeService.ObtenerPorGuidAsync(guid)
                ?? throw new NotFoundException("Incluye", guid);

            model.IncDescripcion = request.Descripcion.Trim();
            await _incluyeService.ActualizarAsync(model);
            return Ok(new ApiItemResponse<IncluyeResponse>(ToIncluyeResponse(model)));
        }

        [HttpDelete("incluye/{guid:guid}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> EliminarIncluye(Guid guid)
        {
            var model = await _incluyeService.ObtenerPorGuidAsync(guid)
                ?? throw new NotFoundException("Incluye", guid);

            await _incluyeService.EliminarLogicoAsync(model.IncId);
            return NoContent();
        }

        private static CategoriaResponse ToCategoriaResponse(CategoriaDataModel c) => new()
        {
            CatGuid = c.CatGuid.ToString(),
            Nombre = c.CatNombre,
            ParentGuid = c.ParentGuid?.ToString(),
            ParentNombre = c.ParentNombre
        };

        private static IdiomaResponse ToIdiomaResponse(IdiomaDataModel i) => new()
        {
            IdGuid = i.IdGuid.ToString(),
            Descripcion = i.IdDescripcion
        };

        private static IncluyeResponse ToIncluyeResponse(IncluyeDataModel i) => new()
        {
            IncluyeGuid = i.IncGuid.ToString(),
            Descripcion = i.IncDescripcion
        };
    }
}
