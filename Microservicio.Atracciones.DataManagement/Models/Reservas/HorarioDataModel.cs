using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Reservas
{
    public class HorarioDataModel
    {
        public int HorId { get; set; }
        public Guid HorGuid { get; set; }
        public int TckId { get; set; }
        public DateOnly HorFecha { get; set; }
        public TimeOnly HorHoraInicio { get; set; }
        public TimeOnly? HorHoraFin { get; set; }
        public int HorCuposDisponibles { get; set; }
        public char HorEstado { get; set; }

        // Auditoría
        public DateTime HorFechaIngreso { get; set; }
        public string HorUsuarioIngreso { get; set; } = string.Empty;
        public string HorIpIngreso { get; set; } = string.Empty;
        public DateTime? HorFechaMod { get; set; }
        public string? HorUsuarioMod { get; set; }
        public string? HorIpMod { get; set; }
        public DateTime? HorFechaEliminacion { get; set; }
        public string? HorUsuarioEliminacion { get; set; }
        public string? HorIpEliminacion { get; set; }
    }
}
