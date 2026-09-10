using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class IntentoExamen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExamenId { get; set; }

        [Required]
        public int EstudianteId { get; set; }

        public DateTime FechaInicio { get; set; }
            = DateTime.UtcNow;

        public DateTime? FechaFin { get; set; }

        public decimal? Calificacion { get; set; }

        public bool IdentidadVerificada { get; set; }

        public bool Finalizado { get; set; }

        public bool Anulado { get; set; }

        [MaxLength(500)]
        public string? MotivoAnulacion { get; set; }

        public Examen? Examen { get; set; }

        public Estudiante? Estudiante { get; set; }

        public ICollection<Respuesta> Respuestas { get; set; }
            = new List<Respuesta>();

        public ICollection<EventoSeguridad> EventosSeguridad { get; set; }
            = new List<EventoSeguridad>();
    }
}
