using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheConsortiumApp.Models
{
    public class LiquidacionExpensa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConsorcioId { get; set; }

        [ForeignKey("ConsorcioId")]
        public virtual Consorcio? Consorcio { get; set; }

        // Periodo YYYYMM (ej: 202605)
        [Required]
        public int Periodo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGastos { get; set; }

        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        public virtual ICollection<LiquidacionExpensaDetalle> Detalles { get; set; }
            = new List<LiquidacionExpensaDetalle>();
    }
}