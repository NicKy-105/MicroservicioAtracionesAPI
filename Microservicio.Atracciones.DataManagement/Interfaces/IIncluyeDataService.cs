using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IIncluyeDataService { Task<IReadOnlyList<IncluyeDataModel>> ListarActivosAsync(); }
}