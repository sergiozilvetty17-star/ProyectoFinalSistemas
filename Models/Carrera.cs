using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Carrera
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<Estudiante> Estudiantes { get; set; }
            = new List<Estudiante>();
    }
}
