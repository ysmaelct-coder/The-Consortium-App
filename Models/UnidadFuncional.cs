using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // 👈 IMPORTANTE: Para usar IFormFile

namespace TheConsortiumApp.Models
{
    public enum TipoAmbientes
    {
        Monoambiente = 1,
        DosAmbientes = 2,
        TresAmbientes = 3,
        CuatroAmbientes = 4,
        CincoAmbientes = 5
    }

    public class UnidadFuncional
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de unidad es obligatorio")]
        public int NroUnidad { get; set; }

        [Required(ErrorMessage = "El piso y departamento son obligatorios")]
        [Display(Name = "Piso / Dpto")]
        public string PisoDepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar la cantidad de ambientes")]
        [Display(Name = "Ambientes")]
        public TipoAmbientes Ambientes { get; set; }

        public decimal Coeficiente { get; set; }

        [Required(ErrorMessage = "El nombre del propietario es obligatorio")]
        [Display(Name = "Propietario")]
        public string NombrePropietario { get; set; } = string.Empty;

        public string? NombreInquilino { get; set; }
        public string? ArchivoContrato { get; set; }
        public bool EstaAlquilada { get; set; }

        [Required]
        public int ConsorcioId { get; set; }

        [ForeignKey("ConsorcioId")]
        public virtual Consorcio? Consorcio { get; set; }

        [NotMapped]
        public string Propietario => NombrePropietario;

        [NotMapped]
        [Display(Name = "Contrato de Alquiler")]
        public IFormFile? ArchivoInquilino { get; set; }
        public virtual ICollection<ComprobantePago> ComprobantesPagos { get; set; } = [];
    }
}