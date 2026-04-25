using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class AtraccionAdminValidator
    {
        public static void ValidarCrear(CrearAtraccionRequest request)
        {
            var errores = new List<string>();

            if (request.DesGuid == Guid.Empty)
                errores.Add("El GUID del destino es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Nombre))
                errores.Add("El nombre de la atracción es obligatorio.");

            if (request.DuracionMinutos.HasValue && request.DuracionMinutos < 1)
                errores.Add("La duración en minutos debe ser mayor a 0.");

            if (request.PrecioReferencia.HasValue && request.PrecioReferencia < 0)
                errores.Add("El precio de referencia no puede ser negativo.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
