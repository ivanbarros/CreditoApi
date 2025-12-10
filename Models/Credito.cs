using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreditoAPI.Models
{
    [Table("credito")]
    public class Credito
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("numero_credito")]
        [StringLength(50)]
        public string NumeroCredito { get; set; } = string.Empty;

        [Required]
        [Column("numero_nfse")]
        [StringLength(50)]
        public string NumeroNfse { get; set; } = string.Empty;

        [Required]
        [Column("data_constituicao")]
        public DateTime DataConstituicao { get; set; }

        [Required]
        [Column("valor_issqn")]
        public decimal ValorIssqn { get; set; }

        [Required]
        [Column("tipo_credito")]
        [StringLength(50)]
        public string TipoCredito { get; set; } = string.Empty;

        [Required]
        [Column("simples_nacional")]
        public bool SimplesNacional { get; set; }

        [Required]
        [Column("aliquota")]
        public decimal Aliquota { get; set; }

        [Required]
        [Column("valor_faturado")]
        public decimal ValorFaturado { get; set; }

        [Required]
        [Column("valor_deducao")]
        public decimal ValorDeducao { get; set; }

        [Required]
        [Column("base_calculo")]
        public decimal BaseCalculo { get; set; }
    }
}
