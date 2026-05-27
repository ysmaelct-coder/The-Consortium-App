using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
public enum TipoAmbientes
{
    Monoambiente = 1,
    DosAmbientes = 2,
    TresAmbientes = 3
}
namespace TheConsortiumApp.Models
{

    public class UnidadFuncional
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de unidad es obligatorio")]
        public int NroUnidad { get; set; }

        [Required(ErrorMessage = "El piso y departamento son obligatorios")]
        [Display(Name = "Piso / Dpto")]
        public string PisoDepto { get; set; } = string.Empty;

        // Resguardo de la precisión matemática según "Tipos de dato en Net.pdf"
        [Required(ErrorMessage = "Debe seleccionar la cantidad de ambientes")]
        public TipoAmbientes Ambientes { get; set; }
        public decimal Coeficiente { get; set; }

        [Required(ErrorMessage = "El nombre del propietario o inquilino es obligatorio")]
        public string Propietario { get; set; } = string.Empty;

        // Clave Foránea (FK) hacia Consorcio
        [Required]
        public int ConsorcioId { get; set; }

        [ForeignKey("ConsorcioId")]
        public virtual Consorcio? Consorcio { get; set; } // Nombre normalizado por convención limpia
    }
}