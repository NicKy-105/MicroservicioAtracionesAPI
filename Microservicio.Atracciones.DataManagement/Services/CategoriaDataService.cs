using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class CategoriaDataService : ICategoriaDataService
    {
        private readonly AtraccionesDbContext _context;
        public CategoriaDataService(AtraccionesDbContext context) => _context = context;

        public async Task<IReadOnlyList<CategoriaDataModel>> ListarActivasAsync()
        {
            return await _context.Categorias.AsNoTracking()
                .Where(c => c.CatEstado == 'A')
                .Include(c => c.Parent)
                .Select(c => new CategoriaDataModel
                {
                    CatId = c.CatId,
                    CatGuid = c.CatGuid,
                    CatNombre = c.CatNombre,
                    CatParentId = c.CatParentId,
                    ParentGuid = c.Parent != null ? c.Parent.CatGuid : null,
                    ParentNombre = c.Parent != null ? c.Parent.CatNombre : null
                }).ToListAsync();
        }
    }
}