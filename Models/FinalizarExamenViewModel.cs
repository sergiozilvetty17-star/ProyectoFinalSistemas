using System.Collections.Generic;

namespace EcommerceApp.Models
{
    public class FinalizarExamenViewModel
    {
        public int IntentoId { get; set; }

        public Dictionary<int, RespuestaExamenViewModel> Respuestas { get; set; }
            = new();
    }

    public class RespuestaExamenViewModel
    {
        public int? OpcionId { get; set; }

        public string? RespuestaTexto { get; set; }
    }
}
