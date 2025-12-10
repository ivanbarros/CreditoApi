using MassTransit;

namespace CreditoAPI.Infrastructure.Saga;

public class CreditoSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    
    public string NumeroCredito { get; set; } = string.Empty;
    public string NumeroNfse { get; set; } = string.Empty;
    public decimal ValorIssqn { get; set; }
    
    public DateTime DataIntegracao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public DateTime? DataConclusao { get; set; }
    
    public bool IntegracaoCompleta { get; set; }
    public bool ProcessamentoCompleto { get; set; }
    public bool AuditoriaCompleta { get; set; }
    
    public string? MensagemErro { get; set; }
    public int TentativasProcessamento { get; set; }
}
