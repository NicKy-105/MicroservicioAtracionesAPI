using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Atracciones
{
    public class ActualizarAtraccionRequest
    {
        [MaxLength(200)] public string? Nombre { get; set; }
        [MaxLength(2000)] public string? Descripcion { get; set; }
        [MaxLength(300)] public string? Direccion { get; set; }
        [Range(1, int.MaxValue)] public int? DuracionMinutos { get; set; }
        [MaxLength(300)] public string? PuntoEncuentro { get; set; }
        [Range(0, double.MaxValue)] public decimal? PrecioReferencia { get; set; }
        public bool? IncluyeAcompaniante { get; set; }
        public bool? IncluyeTransporte { get; set; }
        public bool? Disponible { get; set; }
    }
}
