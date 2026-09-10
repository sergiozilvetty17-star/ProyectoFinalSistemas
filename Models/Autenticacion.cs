using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Autenticacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; }
            = DateTime.UtcNow;

        public bool Exitosa { get; set; }

        [MaxLength(45)]
        public string? DireccionIP { get; set; }

        [MaxLength(500)]
        public string? Detalle { get; set; }

        public ApplicationUser? Usuario { get; set; }
    }
}
