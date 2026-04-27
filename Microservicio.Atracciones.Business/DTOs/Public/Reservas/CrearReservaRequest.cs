using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Reservas
{
    public class CrearReservaRequest
    {
        [Required(ErrorMessage = "El GUID de la atracción es obligatorio.")]
        public Guid AtGuid { get; set; }

        [Required(ErrorMessage = "El GUID del horario es obligatorio.")]
        public Guid HorGuid { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos una línea de ticket.")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos una línea de ticket.")]
        public IList<ReservaDetalleRequest> Lineas { get; set; } = new List<ReservaDetalleRequest>();

        public string? OrigenCanal { get; set; }
    }
}
