using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Queries;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Catalogos;
using Microservicio.Atracciones.DataManagement.Mappers.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class AtraccionDataService : IAtraccionDataService
    {
        private readonly IUnitOfWork _uow;
        private readonly AtraccionQueryRepository _queryRepo;
        private readonly TicketQueryRepository _ticketQuery;

        public AtraccionDataService(
            IUnitOfWork uow,
            AtraccionQueryRepository queryRepo,
            TicketQueryRepository ticketQuery)
        {
            _uow = uow;
            _queryRepo = queryRepo;
            _ticketQuery = ticketQuery;
        }

        public async Task<AtraccionDataModel?> ObtenerPorIdAsync(int atId)
        {
            var entity = await _uow.Atracciones.ObtenerPorIdAsync(atId);
            if (entity is null) return null;

            var precioDesde = await _ticketQuery.ObtenerPrecioDesdeAsync(atId);
            var calificacion = entity.Resenias.Where(r => r.RsnEstado == 'A').Any()
                ? entity.Resenias.Where(r => r.RsnEstado == 'A').Average(r => (double)r.RsnRating)
                : (double?)null;

            return AtraccionDataMapper.ToDataModel(entity, precioDesde, calificacion);
        }

        public async Task<AtraccionDataModel?> ObtenerPorGuidAsync(Guid atGuid)
        {
            var entity = await _queryRepo.ObtenerDetallePublicoAsync(atGuid);
            if (entity is null) return null;

            var precioDesde = await _ticketQuery.ObtenerPrecioDesdeAsync(entity.AtId);
            var calificacion = entity.Resenias.Where(r => r.RsnEstado == 'A').Any()
                ? entity.Resenias.Where(r => r.RsnEstado == 'A').Average(r => (double)r.RsnRating)
                : (double?)null;

            return AtraccionDataMapper.ToDataModel(entity, precioDesde, calificacion);
        }

        public async Task<DataPagedResult<AtraccionDataModel>> ListarConFiltrosAsync(AtraccionFiltroDataModel filtro)
        {
            // Mapea el filtro DataManagement → filtro DataAccess
            var filtroQuery = new AtraccionFiltroQuery(
                Ciudad: filtro.Ciudad,
                TipoCatGuid: filtro.TipoCatGuid,
                SubtipoCatGuid: filtro.SubtipoCatGuid,
                Etiqueta: filtro.Etiqueta,
                Idioma: filtro.Idioma,
                CalificacionMin: filtro.CalificacionMin,
                Horario: filtro.Horario,
                Disponible: filtro.Disponible,
                OrdenarPor: filtro.OrdenarPor,
                Page: filtro.Page,
                Limit: filtro.Limit
            );

            var paged = await _queryRepo.ListarConFiltrosAsync(filtroQuery);

            // Para cada atracción obtenemos precio mínimo y calificación
            var items = new List<AtraccionDataModel>();
            foreach (var entity in paged.Items)
            {
                var precioDesde = await _ticketQuery.ObtenerPrecioDesdeAsync(entity.AtId);
                var calificacion = entity.Resenias.Where(r => r.RsnEstado == 'A').Any()
                    ? entity.Resenias.Where(r => r.RsnEstado == 'A').Average(r => (double)r.RsnRating)
                    : (double?)null;

                items.Add(AtraccionDataMapper.ToDataModel(entity, precioDesde, calificacion)!);
            }

            return new DataPagedResult<AtraccionDataModel>(
                items, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task CrearAsync(AtraccionDataModel model)
        {
            var entity = AtraccionDataMapper.ToNewEntity(model);
            await _uow.Atracciones.AgregarAsync(entity);
            await _uow.SaveChangesAsync();

            model.AtId = entity.AtId;
            model.AtGuid = entity.AtGuid;
        }

        public async Task ActualizarAsync(AtraccionDataModel model)
        {
            var entity = await _uow.Atracciones.ObtenerPorIdAsync(model.AtId)
                ?? throw new InvalidOperationException($"Atraccion {model.AtId} no encontrada.");

            AtraccionDataMapper.ApplyToEntity(model, entity);
            _uow.Atracciones.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int atId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Atracciones.ObtenerPorIdAsync(atId)
                ?? throw new InvalidOperationException($"Atraccion {atId} no encontrada.");

            entity.AtEstado = 'I';
            entity.AtFechaEliminacion = DateTime.UtcNow;
            entity.AtUsuarioEliminacion = usuarioAccion;
            entity.AtIpEliminacion = ip;

            _uow.Atracciones.Actualizar(entity);
            await _uow.SaveChangesAsync();
        }

        // E-03: Consulta dedicada para categorías raíz con sus hijos (para type_filters jerárquicos)
        public async Task<IReadOnlyList<CategoriaDataModel>> ObtenerCategoriasPorCiudadAsync(string ciudad)
        {
            var raices = await _queryRepo.ObtenerCategoriasPorCiudadAsync(ciudad);

            return raices.Select(c => new CategoriaDataModel
            {
                CatId       = c.CatId,
                CatGuid     = c.CatGuid,
                CatParentId = c.CatParentId,
                CatNombre   = c.CatNombre,
                CatEstado   = c.CatEstado,
                // Mapear los hijos integrados en la query
                Hijos = c.Hijos.Select(h => new CategoriaDataModel
                {
                    CatId       = h.CatId,
                    CatGuid     = h.CatGuid,
                    CatParentId = h.CatParentId,
                    CatNombre   = h.CatNombre,
                    CatEstado   = h.CatEstado
                }).ToList()
            }).ToList();
        }

        // ----------------------------------------------------------------
        //  Gestión de relaciones N:M
        // ----------------------------------------------------------------
        public async Task AgregarCategoriaAsync(int atId, int catId, string usuarioAccion, string ip)
        {
            await _uow.Atracciones.AgregarCategoriaAsync(new CategoriaAtraccionEntity
            {
                AtId = atId,
                CatId = catId,
                CaEstado = 'A',
                CaFechaIngreso = DateTime.UtcNow,
                CaUsuarioIngreso = usuarioAccion
            });
            await _uow.SaveChangesAsync();
        }

        public async Task AgregarIdiomaAsync(int atId, int idId, string usuarioAccion, string ip)
        {
            await _uow.Atracciones.AgregarIdiomaAsync(new IdiomaAtraccionEntity
            {
                AtId = atId,
                IdId = idId,
                IaEstado = 'A',
                IaFechaIngreso = DateTime.UtcNow,
                IaUsuarioIngreso = usuarioAccion
            });
            await _uow.SaveChangesAsync();
        }

        public async Task AgregarImagenAsync(int atId, int imgId, string usuarioAccion, string ip)
        {
            await _uow.Atracciones.AgregarImagenAsync(new ImagenAtraccionEntity
            {
                AtId = atId,
                ImgId = imgId,
                ImaEstado = 'A',
                ImaFechaIngreso = DateTime.UtcNow,
                ImaUsuarioIngreso = usuarioAccion
            });
            await _uow.SaveChangesAsync();
        }

        public async Task AgregarIncluyeAsync(int atId, int incId, string usuarioAccion, string ip)
        {
            await _uow.Atracciones.AgregarIncluyeAsync(new AtraccionIncluyeEntity
            {
                AtId = atId,
                IncId = incId,
                AiEstado = 'A',
                AiFechaIngreso = DateTime.UtcNow,
                AiUsuarioIngreso = usuarioAccion
            });
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarCategoriaAsync(int atId, int catId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Atracciones.ObtenerPorIdAsync(atId);
            var pivote = entity?.CategoriasAtracciones.FirstOrDefault(ca => ca.CatId == catId);
            if (pivote is null) return;

            pivote.CaEstado = 'I';
            pivote.CaFechaEliminacion = DateTime.UtcNow;
            pivote.CaUsuarioEliminacion = usuarioAccion;
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarIdiomaAsync(int atId, int idId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Atracciones.ObtenerPorIdAsync(atId);
            var pivote = entity?.IdiomasAtracciones.FirstOrDefault(ia => ia.IdId == idId);
            if (pivote is null) return;

            pivote.IaEstado = 'I';
            pivote.IaFechaEliminacion = DateTime.UtcNow;
            pivote.IaUsuarioEliminacion = usuarioAccion;
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarImagenAsync(int atId, int imgId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Atracciones.ObtenerPorIdAsync(atId);
            var pivote = entity?.ImagenesAtracciones.FirstOrDefault(ima => ima.ImgId == imgId);
            if (pivote is null) return;

            pivote.ImaEstado = 'I';
            pivote.ImaFechaEliminacion = DateTime.UtcNow;
            pivote.ImaUsuarioEliminacion = usuarioAccion;
            await _uow.SaveChangesAsync();
        }

        public async Task EliminarIncluyeAsync(int atId, int incId, string usuarioAccion, string ip)
        {
            var entity = await _uow.Atracciones.ObtenerPorIdAsync(atId);
            var pivote = entity?.AtraccionesIncluyen.FirstOrDefault(ai => ai.IncId == incId);
            if (pivote is null) return;

            pivote.AiEstado = 'I';
            pivote.AiFechaEliminacion = DateTime.UtcNow;
            pivote.AiUsuarioEliminacion = usuarioAccion;
            await _uow.SaveChangesAsync();
        }
    }
}
