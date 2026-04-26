using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class IncluyeDataService : IIncluyeDataService
    {
        private readonly AtraccionesDbContext _context;
        public IncluyeDataService(AtraccionesDbContext context) => _context = context;

        public async Task<IReadOnlyList<IncluyeDataModel>> ListarActivosAsync()
        {
            return await _context.Incluyes.AsNoTracking()
                .Where(c => c.IncEstado == 'A')
                .Select(c => new IncluyeDataModel
                {
                    IncId = c.IncId,
                    IncGuid = c.IncGuid,
                    IncDescripcion = c.IncDescripcion
                }).ToListAsync();
        }
    }
}