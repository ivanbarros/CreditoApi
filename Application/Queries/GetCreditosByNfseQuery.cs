using CreditoAPI.DTOs;
using MediatR;

namespace CreditoAPI.Application.Queries
{
    /// <summary>
    /// Query para buscar créditos por número de NFS-e
    /// Implementa CQRS pattern - lado de leitura
    /// </summary>
    public class GetCreditosByNfseQuery : IRequest<List<CreditoDto>>
    {
        public string NumeroNfse { get; set; }

        public GetCreditosByNfseQuery(string numeroNfse)
        {
            NumeroNfse = numeroNfse;
        }
    }
}
