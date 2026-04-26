using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class IdiomaDataService : IIdiomaDataService
    {
        private readonly AtraccionesDbContext _context;
        public IdiomaDataService(AtraccionesDbContext context) => _context = context;

        public async Task<IReadOnlyList<IdiomaDataModel>> ListarActivosAsync()
        {
            return await _context.Idiomas.AsNoTracking()
                .Where(c => c.IdEstado == 'A')
                .Select(c => new IdiomaDataModel
                {
                    IdId = c.IdId,
                    IdGuid = c.IdGuid,
                    IdDescripcion = c.IdDescripcion
                }).ToListAsync();
        }
    }
}