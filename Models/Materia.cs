using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Materia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public int? DocenteId { get; set; }

        public bool Activa { get; set; } = true;

        public Docente? Docente { get; set; }

        public ICollection<Inscripcion> Inscripciones { get; set; }
            = new List<Inscripcion>();
    }
}
