using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IIdiomaDataService { Task<IReadOnlyList<IdiomaDataModel>> ListarActivosAsync(); }
}