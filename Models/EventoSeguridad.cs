using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class EventoSeguridad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public int? IntentoExamenId { get; set; }

        [Required]
        public TipoEvento Tipo { get; set; }

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        public DateTime FechaHora { get; set; }
            = DateTime.UtcNow;

        [MaxLength(45)]
        public string? DireccionIP { get; set; }

        public ApplicationUser? Usuario { get; set; }

        public IntentoExamen? IntentoExamen { get; set; }
    }
}
