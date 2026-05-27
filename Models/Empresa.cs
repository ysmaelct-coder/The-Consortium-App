using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace TheConsortiumApp.Models
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria")]
        [StringLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El CUIT debe tener exactamente 11 dígitos")]
        public string Cuit { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Propiedad de navegación: Una Empresa maneja muchos Consorcios (Relación 1 a Muchos)
        public virtual ICollection<Consorcio> Consorcios { get; set; } = new List<Consorcio>();
    }
}