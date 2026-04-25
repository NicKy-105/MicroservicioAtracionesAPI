using Microservicio.Atracciones.DataAccess.Entities.Seguridad;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<UsuarioEntity?> ObtenerPorIdAsync(int usuId);
        Task<UsuarioEntity?> ObtenerPorLoginAsync(string login);
        Task AgregarAsync(UsuarioEntity usuario);
        void Actualizar(UsuarioEntity usuario);
    }
}
