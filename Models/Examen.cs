using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Examen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MateriaId { get; set; }

        [Required]
        public string DocenteId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        [Range(1, 600)]
        public int DuracionMinutos { get; set; } = 60;

        public EstadoExamen Estado { get; set; } = EstadoExamen.Borrador;

        public bool RequiereVerificacionFacial { get; set; } = true;

        public bool Activo { get; set; } = true;

        public Materia? Materia { get; set; }

        public ApplicationUser? Docente { get; set; }

        public ICollection<Pregunta> Preguntas { get; set; }
            = new List<Pregunta>();

        public ICollection<IntentoExamen> Intentos { get; set; }
            = new List<IntentoExamen>();
    }
}
