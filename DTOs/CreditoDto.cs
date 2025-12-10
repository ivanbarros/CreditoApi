using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CreditoAPI.DTOs
{
    public class CreditoDto
    {
        [Required]
        [JsonPropertyName("numeroCredito")]
        public string NumeroCredito { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("numeroNfse")]
        public string NumeroNfse { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("dataConstituicao")]
        public string DataConstituicao { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("valorIssqn")]
        public decimal ValorIssqn { get; set; }

        [Required]
        [JsonPropertyName("tipoCredito")]
        public string TipoCredito { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("simplesNacional")]
        public string SimplesNacional { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("aliquota")]
        public decimal Aliquota { get; set; }

        [Required]
        [JsonPropertyName("valorFaturado")]
        public decimal ValorFaturado { get; set; }

        [Required]
        [JsonPropertyName("valorDeducao")]
        public decimal ValorDeducao { get; set; }

        [Required]
        [JsonPropertyName("baseCalculo")]
        public decimal BaseCalculo { get; set; }
    }

    public class IntegrarCreditoRequest
    {
        [Required]
        public List<CreditoDto> Creditos { get; set; } = new List<CreditoDto>();
    }

    public class IntegrarCreditoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
