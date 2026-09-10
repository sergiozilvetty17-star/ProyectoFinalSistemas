using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Respuesta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IntentoExamenId { get; set; }

        [Required]
        public int PreguntaId { get; set; }

        public int? OpcionId { get; set; }

        [MaxLength(4000)]
        public string? RespuestaTexto { get; set; }

        public decimal PuntajeObtenido { get; set; }

        public bool Correcta { get; set; }

        public IntentoExamen? IntentoExamen { get; set; }

        public Pregunta? Pregunta { get; set; }

        public Opcion? Opcion { get; set; }
    }
}
