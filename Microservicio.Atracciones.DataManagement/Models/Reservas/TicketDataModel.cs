using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Reservas
{
    public class TicketDataModel
    {
        public int TckId { get; set; }
        public Guid TckGuid { get; set; }
        public int AtId { get; set; }
        public string TckTitulo { get; set; } = string.Empty;
        public decimal TckPrecio { get; set; }
        public string TckTipoParticipante { get; set; } = string.Empty;
        public int TckCapacidadMaxima { get; set; }
        public int TckCuposDisponibles { get; set; }
        public char TckEstado { get; set; }

        // Auditoría
        public DateTime TckFechaIngreso { get; set; }
        public string TckUsuarioIngreso { get; set; } = string.Empty;
        public string TckIpIngreso { get; set; } = string.Empty;
        public DateTime? TckFechaMod { get; set; }
        public string? TckUsuarioMod { get; set; }
        public string? TckIpMod { get; set; }
        public DateTime? TckFechaEliminacion { get; set; }
        public string? TckUsuarioEliminacion { get; set; }
        public string? TckIpEliminacion { get; set; }
    }
}
