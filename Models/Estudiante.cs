using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Estudiante
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public int CarreraId { get; set; }

        [Range(1, 20)]
        public int? Semestre { get; set; }

        public bool Activo { get; set; } = true;

        public ApplicationUser? Usuario { get; set; }

        public Carrera? Carrera { get; set; }

        public ICollection<Inscripcion> Inscripciones { get; set; }
            = new List<Inscripcion>();
    }
}
