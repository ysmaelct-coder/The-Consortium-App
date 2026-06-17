using System.ComponentModel.DataAnnotations;

namespace TheConsortiumApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Relación necesaria para el filtro de gastos
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public ICollection<Consorcio> Consorcios { get; set; } = new List<Consorcio>();
        public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
    }
}