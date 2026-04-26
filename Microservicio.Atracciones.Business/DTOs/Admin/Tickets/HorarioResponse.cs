namespace Microservicio.Atracciones.Business.DTOs.Admin.Tickets
{
    public class HorarioResponse
    {
        public string HorGuid { get; set; } = string.Empty;
        public string TckGuid { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;          // "YYYY-MM-DD"
        public string HoraInicio { get; set; } = string.Empty;     // "HH:mm"
        public string? HoraFin { get; set; }
        public int CuposDisponibles { get; set; }
        public char Estado { get; set; }
    }
}
