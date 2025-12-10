using CreditoAPI.DTOs;
using MediatR;

namespace CreditoAPI.Application.Queries
{
    /// <summary>
    /// Query para buscar um crédito por número
    /// Implementa CQRS pattern - lado de leitura
    /// </summary>
    public class GetCreditoByNumeroQuery : IRequest<CreditoDto?>
    {
        public string NumeroCredito { get; set; }

        public GetCreditoByNumeroQuery(string numeroCredito)
        {
            NumeroCredito = numeroCredito;
        }
    }
}
