using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class PreguntaCreateViewModel
    {
        [Required]
        public int ExamenId { get; set; }

        [Required(ErrorMessage = "El enunciado es obligatorio.")]
        [MaxLength(2000)]
        [Display(Name = "Enunciado")]
        public string Enunciado { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tipo de pregunta")]
        public TipoPregunta Tipo { get; set; }

        [Range(0.01, 1000, ErrorMessage = "El puntaje debe ser mayor a 0.")]
        [Display(Name = "Puntaje")]
        public decimal Puntaje { get; set; } = 1;

        [Range(1, 10000)]
        [Display(Name = "Orden")]
        public int Orden { get; set; } = 1;
    }
}
