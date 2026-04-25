using Microservicio.Atracciones.Business.Interfaces.Auth;

namespace Microservicio.Atracciones.Api.Services
{
    /// <summary>
    /// Implementación v1: comparación de texto plano (sin hash).
    /// Para habilitar BCrypt en producción, reemplazar el cuerpo de ambos
    /// métodos por BCrypt.Net.BCrypt.HashPassword / Verify.
    /// La interfaz IPasswordHasher (capa Business) no cambia.
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string Hashear(string password)
            => password;   // v1: sin hashing

        public bool Verificar(string password, string hash)
            => password == hash;   // v1: comparación directa
    }
}
