using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace TheConsortiumApp.Models
{
    public class Consorcio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del consorcio es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La localidad es obligatoria")]
        public string Localidad { get; set; } = string.Empty;

        // Clave Foránea (FK): Conecta el consorcio con la Empresa dueña
        [Required]
        public int EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; } // Nombre normalizado para acoplamiento por convención

        // Un Consorcio contiene muchas Unidades Funcionales (Relación 1 a Muchos)
        public virtual ICollection<UnidadFuncional> UnidadesFuncionales { get; set; } = new List<UnidadFuncional>();
        public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
    }
}