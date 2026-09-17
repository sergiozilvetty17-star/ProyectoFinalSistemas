using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class OpcionCreateViewModel
    {
        [Required]
        public int PreguntaId { get; set; }

        [Required(ErrorMessage = "El texto de la opción es obligatorio.")]
        [MaxLength(1000)]
        [Display(Name = "Texto de la opción")]
        public string Texto { get; set; } = string.Empty;

        [Display(Name = "Respuesta correcta")]
        public bool EsCorrecta { get; set; }

        [Range(1, 100)]
        [Display(Name = "Orden")]
        public int Orden { get; set; } = 1;
    }
}
