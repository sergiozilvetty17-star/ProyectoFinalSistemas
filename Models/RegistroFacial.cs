using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class RegistroFacial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EstudianteId { get; set; }

        [Required]
        public byte[] FaceEmbedding { get; set; } = Array.Empty<byte>();

        public DateTime FechaRegistro { get; set; }
            = DateTime.UtcNow;

        public bool Activo { get; set; } = true;

        public Estudiante? Estudiante { get; set; }
    }
}
