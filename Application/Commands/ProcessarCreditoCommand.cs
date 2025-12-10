using CreditoAPI.DTOs;
using MediatR;

namespace CreditoAPI.Application.Commands
{
    /// <summary>
    /// Command para processar um crédito individual recebido do Service Bus
    /// </summary>
    public class ProcessarCreditoCommand : IRequest<bool>
    {
        public CreditoDto Credito { get; set; }

        public ProcessarCreditoCommand(CreditoDto credito)
        {
            Credito = credito;
        }
    }
}
