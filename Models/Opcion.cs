using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Opcion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PreguntaId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Texto { get; set; } = string.Empty;

        public bool EsCorrecta { get; set; }

        public int Orden { get; set; }

        public Pregunta? Pregunta { get; set; }

        public ICollection<Respuesta> Respuestas { get; set; }
            = new List<Respuesta>();
    }
}
