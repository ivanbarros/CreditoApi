using CreditoAPI.Models;

namespace CreditoAPI.Infrastructure.Specifications
{
    /// <summary>
    /// Specification para buscar créditos por NFS-e
    /// Encapsula lógica de consulta reutilizável
    /// </summary>
    public class CreditosByNfseSpecification : BaseSpecification<Credito>
    {
        public CreditosByNfseSpecification(string numeroNfse)
            : base(c => c.NumeroNfse == numeroNfse)
        {
            ApplyOrderBy(c => c.DataConstituicao);
        }
    }
}
