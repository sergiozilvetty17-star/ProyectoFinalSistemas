using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EcommerceApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ApellidoMaterno { get; set; }

        [MaxLength(20)]
        public string? CedulaIdentidad { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<ApplicationUser> CreatedUsers { get; set; }
            = new List<ApplicationUser>();

        public string NombreCompleto =>
            string.Join(
                " ",
                new[]
                {
                    Nombres,
                    ApellidoPaterno,
                    ApellidoMaterno
                }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
