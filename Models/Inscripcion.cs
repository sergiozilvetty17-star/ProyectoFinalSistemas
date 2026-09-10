using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Inscripcion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EstudianteId { get; set; }

        [Required]
        public int MateriaId { get; set; }

        public DateTime FechaInscripcion { get; set; }
            = DateTime.UtcNow;

        public bool Activa { get; set; } = true;

        public Estudiante? Estudiante { get; set; }

        public Materia? Materia { get; set; }
    }
}
