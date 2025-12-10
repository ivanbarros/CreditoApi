using CreditoAPI.Models;

namespace CreditoAPI.Infrastructure.Specifications
{
    /// <summary>
    /// Specification para buscar crédito por número
    /// Encapsula lógica de consulta reutilizável
    /// </summary>
    public class CreditoByNumeroSpecification : BaseSpecification<Credito>
    {
        public CreditoByNumeroSpecification(string numeroCredito)
            : base(c => c.NumeroCredito == numeroCredito)
        {
        }
    }
}
