using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Atracciones
{
    public class AtraccionAdminResponse
    {
        public string AtGuid { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? DuracionMinutos { get; set; }
        public decimal? PrecioReferencia { get; set; }
        public bool Disponible { get; set; }
        public char Estado { get; set; }
        public int TotalResenias { get; set; }
        public DateTime FechaIngreso { get; set; }
    }
}
