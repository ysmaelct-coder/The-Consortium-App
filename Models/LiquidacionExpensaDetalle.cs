using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheConsortiumApp.Models
{
    public class LiquidacionExpensaDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LiquidacionExpensaId { get; set; }

        [ForeignKey("LiquidacionExpensaId")]
        public virtual LiquidacionExpensa? Liquidacion { get; set; }

        [Required]
        public int UnidadFuncionalId { get; set; }

        [ForeignKey("UnidadFuncionalId")]
        public virtual UnidadFuncional? UnidadFuncional { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Coeficiente { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoCalculado { get; set; }
    }
}