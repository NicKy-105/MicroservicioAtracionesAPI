using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Catalogos;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
    }
}
