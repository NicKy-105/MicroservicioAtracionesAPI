using Microservicio.Atracciones.DataManagement.Models.Seguridad;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IUsuarioDataService
    {
        Task<UsuarioDataModel?> ObtenerPorIdAsync(int usuId);
        Task<UsuarioDataModel?> ObtenerPorLoginAsync(string login);
        Task<IReadOnlyList<RolDataModel>> ObtenerRolesPorUsuarioAsync(int usuId);
        Task CrearAsync(UsuarioDataModel model);
        Task ActualizarAsync(UsuarioDataModel model);
        Task EliminarLogicoAsync(int usuId, string usuarioAccion, string ip);
        Task<bool> ExisteLoginAsync(string login);
    }
}
