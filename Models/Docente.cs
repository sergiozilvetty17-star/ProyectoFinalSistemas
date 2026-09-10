using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Docente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Especialidad { get; set; }

        public bool Activo { get; set; } = true;

        public ApplicationUser? Usuario { get; set; }

        public ICollection<Materia> Materias { get; set; }
            = new List<Materia>();
    }
}
