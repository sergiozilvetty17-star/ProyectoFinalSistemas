using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Pregunta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExamenId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Enunciado { get; set; } = string.Empty;

        public TipoPregunta Tipo { get; set; }

        [Range(0.01, 1000)]
        public decimal Puntaje { get; set; } = 1;

        public int Orden { get; set; }

        public Examen? Examen { get; set; }

        public ICollection<Opcion> Opciones { get; set; }
            = new List<Opcion>();

        public ICollection<Respuesta> Respuestas { get; set; }
            = new List<Respuesta>();
    }
}
