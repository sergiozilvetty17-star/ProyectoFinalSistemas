using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class InscripcionCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Display(Name = "Estudiante")]
        public int EstudianteId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una materia.")]
        [Display(Name = "Materia")]
        public int MateriaId { get; set; }
    }
}
