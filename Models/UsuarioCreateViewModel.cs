using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class UsuarioCreateViewModel
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [Display(Name = "Nombres")]
        [MaxLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [Display(Name = "Apellido paterno")]
        [MaxLength(100)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Display(Name = "Apellido materno")]
        [MaxLength(100)]
        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "La C.I. es obligatoria.")]
        [Display(Name = "Cédula de identidad")]
        [MaxLength(20)]
        public string CedulaIdentidad { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione un rol.")]
        public string Rol { get; set; } = string.Empty;

        [Display(Name = "Carrera")]
        public int? CarreraId { get; set; }

        [Display(Name = "Semestre")]
        [Range(1, 20, ErrorMessage = "Seleccione un semestre válido.")]
        public int? Semestre { get; set; }

        [Display(Name = "Especialidad")]
        [MaxLength(150)]
        public string? Especialidad { get; set; }
    }
}
