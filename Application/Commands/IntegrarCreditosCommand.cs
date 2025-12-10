using CreditoAPI.DTOs;
using MediatR;

namespace CreditoAPI.Application.Commands
{
    /// <summary>
    /// Command para integrar créditos constituídos no sistema
    /// Implementa CQRS pattern separando comandos de consultas
    /// </summary>
    public class IntegrarCreditosCommand : IRequest<IntegrarCreditoResponse>
    {
        public List<CreditoDto> Creditos { get; set; }

        public IntegrarCreditosCommand(List<CreditoDto> creditos)
        {
            Creditos = creditos ?? new List<CreditoDto>();
        }
    }
}
