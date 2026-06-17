using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TheConsortiumApp.Models
{
    public class ComprobantePago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UnidadFuncionalId { get; set; }

        [ForeignKey("UnidadFuncionalId")]
        public virtual UnidadFuncional? UnidadFuncional { get; set; }

        [Required]
        [Display(Name = "Período Mensual")]
        public string? Periodo { get; set; } // Ejemplo: "2026-06" o "Junio 2026"

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Pago Realizada")]
        public DateTime FechaPago { get; set; }

        //  Comprobantes físicos guardados en el disco
        public string? ArchivoAlquiler { get; set; }
        public string? ArchivoExpensas { get; set; }

        //  Propiedades temporales para capturar los archivos en el formulario
        [NotMapped]
        public IFormFile? InputArchivoAlquiler { get; set; }

        [NotMapped]
        public IFormFile? InputArchivoExpensas { get; set; }
    }
}