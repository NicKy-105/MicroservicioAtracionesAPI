using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.Business.Mappers.Public;
using Microservicio.Atracciones.Business.Validators.Public;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Services.Public
{
    public class AtraccionPublicService : IAtraccionPublicService
    {
        private readonly IAtraccionDataService _atraccionService;
        private readonly ITicketDataService _ticketService;
        private readonly IDestinoDataService _destinoService;

        public AtraccionPublicService(
            IAtraccionDataService atraccionService,
            ITicketDataService ticketService,
            IDestinoDataService destinoService)
        {
            _atraccionService = atraccionService;
            _ticketService = ticketService;
            _destinoService = destinoService;
        }

        public async Task<DataPagedResult<AtraccionListadoResponse>> ListarAsync(
            AtraccionFiltroRequest request, string baseUrl)
        {
            AtraccionPublicValidator.Validar(request);

            var filtro = new AtraccionFiltroDataModel
            {
                Ciudad        = request.Ciudad,
                TipoCatGuid   = request.Tipo    is not null ? Guid.Parse(request.Tipo)    : null,
                SubtipoCatGuid= request.Subtipo is not null ? Guid.Parse(request.Subtipo) : null,
                Etiqueta      = request.Etiqueta,
                Idioma        = request.Idioma,
                CalificacionMin = request.CalificacionMin,
                Horario       = request.Horario,
                Disponible    = request.Disponible,
                OrdenarPor    = request.OrdenarPor,
                Page          = request.Page,
                Limit         = request.Limit
            };

            var paged = await _atraccionService.ListarConFiltrosAsync(filtro);

            // #4: batch query — una sola llamada en lugar de N queries independientes
            var atIds    = paged.Items.Select(a => a.AtId).ToList();
            var dispBatch = await _ticketService.ObtenerDisponibilidadBatchAsync(atIds);

            var responses = paged.Items
                .Select(model =>
                {
                    dispBatch.TryGetValue(model.AtId, out var disp);
                    return AtraccionPublicMapper.ToListadoResponse(model, disp, baseUrl);
                })
                .ToList();

            return new DataPagedResult<AtraccionListadoResponse>(
                responses, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task<AtraccionDetalleResponse> ObtenerPorGuidAsync(Guid atGuid, string baseUrl)
        {
            var model = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            var disponibilidad = await _ticketService.ObtenerDisponibilidadAsync(model.AtId);
            var tickets  = (await _ticketService.ListarPorAtraccionAsync(model.AtId)).ToList();
            var horarios = (await _ticketService.ListarHorariosPorAtraccionAsync(model.AtId)).ToList();

            return AtraccionPublicMapper.ToDetalleResponse(
                model, disponibilidad, tickets, horarios, baseUrl, model.DesNombre);
        }

        public async Task<FiltrosAtraccionResponse> ObtenerFiltrosAsync(string? ciudad)
        {
            AtraccionPublicValidator.ValidarCiudadFiltros(ciudad);

            // #5: Una sola carga — categorías, etiquetas e idiomas se derivan de esta lista
            var atracciones = (await _atraccionService.ListarConFiltrosAsync(
                new AtraccionFiltroDataModel { Ciudad = ciudad, Limit = 200 })).Items;

            var total = atracciones.Count;
            var destinos = string.IsNullOrWhiteSpace(ciudad)
                ? await _destinoService.ListarActivosAsync()
                : (await _destinoService.ObtenerPorNombreAsync(ciudad)) is { } destino
                    ? new[] { destino }
                    : Array.Empty<DataManagement.Models.Catalogos.DestinoDataModel>();

            // #8: Count real por cada destino — ya no siempre 0 para los que no son la ciudad buscada
            var destinationFilters = destinos
                .Select(destino => new OpcionFiltroResponse
                {
                    Name         = destino.DesNombre,
                    Tagname      = destino.DesGuid.ToString(),
                    ProductCount = atracciones.Count(a => a.DesId == destino.DesId),
                    Image        = destino.DesImagenUrl is not null
                                   ? new ImagenFiltroResponse { Url = destino.DesImagenUrl }
                                   : null
                })
                .ToList();

            // #5: Todos los helpers usan la misma lista cargada (excepto categorías que usa query optimizada)
            // E-03: Obtener estructura jerárquica real desde BD y contar atracciones
            var categoriasBD = await _atraccionService.ObtenerCategoriasPorCiudadAsync(ciudad);
            var categorias = new List<OpcionFiltroResponse>();
            foreach (var cat in categoriasBD)
            {
                var hijos = cat.Hijos.Select(h => new OpcionFiltroResponse
                {
                    Name = h.CatNombre,
                    Tagname = h.CatGuid.ToString(),
                    ProductCount = atracciones.Count(a => a.Categorias.Any(ac => ac.CatId == h.CatId)),
                    Image = null
                }).ToList();

                var productCount = atracciones.Count(a => a.Categorias.Any(ac => ac.CatId == cat.CatId || cat.Hijos.Any(h => h.CatId == ac.CatId)));

                categorias.Add(new OpcionFiltroResponse
                {
                    Name = cat.CatNombre,
                    Tagname = cat.CatGuid.ToString(),
                    ProductCount = productCount,
                    Image = null,
                    ChildFilterOptions = hijos.Any() ? hijos : null
                });
            }

            var etiquetasConteo = atracciones
                .SelectMany(a => a.Incluyes)
                .Where(i => !i.IncDescripcion.StartsWith("NO:"))
                .GroupBy(i => i.IncDescripcion)
                .Select(g => (Descripcion: g.Key, Conteo: g.Count()))
                .ToList();

            var idiomasConteo = atracciones
                .SelectMany(a => a.Idiomas)
                .GroupBy(i => i.IdDescripcion)
                .Select(g => (Descripcion: g.Key, Conteo: g.Count()))
                .ToList();

            // #2: MinRatingFilter — counts reales basados en calificación promedio de la ciudad
            var minRatingFilters = new[] { 4.5, 4.0, 3.5, 3.0 }
                .Select(r => new OpcionFiltroResponse
                {
                    Name         = $"{r:F1} y más",
                    Tagname      = $"{r:F1}",
                    ProductCount = atracciones.Count(a =>
                        a.CalificacionPromedio.HasValue &&
                        a.CalificacionPromedio.Value >= r)
                }).ToList();

            // #2 + #10: TimeOfDayFilters — los horarios no están en AtraccionDataModel
            // (requeriría una query adicional por atracción — se incluye la estructura con count real
            //  en versión futura cuando se agregue HorariosResumen al data model)
            var timeOfDayFilters = new List<OpcionFiltroResponse>
            {
                new() { Name = "Mañanas", Tagname = "05:00-12:00", ProductCount = 0 },
                new() { Name = "Tardes",  Tagname = "12:00-18:00", ProductCount = 0 },
                new() { Name = "Noches",  Tagname = "18:00-05:00", ProductCount = 0 },
            };

            return new FiltrosAtraccionResponse
            {
                DestinationFilters = destinationFilters,
                TypeFilters = categorias,
                LabelFilters = etiquetasConteo.Select(e => new OpcionFiltroResponse
                {
                    Name = e.Descripcion, Tagname = e.Descripcion, ProductCount = e.Conteo
                }).ToList(),
                MinRatingFilter = minRatingFilters,
                TimeOfDayFilters = timeOfDayFilters,
                SupportedLanguageFilters = idiomasConteo.Select(i => new OpcionFiltroResponse
                {
                    Name = i.Descripcion, Tagname = i.Descripcion, ProductCount = i.Conteo
                }).ToList(),
                UfiFilters = new List<OpcionFiltroResponse>
                {
                    new()
                    {
                        Name = string.IsNullOrWhiteSpace(ciudad) ? "Todos" : ciudad,
                        Tagname = string.IsNullOrWhiteSpace(ciudad) ? "todos" : ciudad.ToLower(),
                        ProductCount = total
                    }
                }
            };
        }


    }
}
