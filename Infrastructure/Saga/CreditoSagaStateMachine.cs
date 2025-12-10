using MassTransit;

namespace CreditoAPI.Infrastructure.Saga;

public class CreditoSagaStateMachine : MassTransitStateMachine<CreditoSagaState>
{
    public State Integrando { get; private set; } = null!;
    public State Processando { get; private set; } = null!;
    public State Auditando { get; private set; } = null!;
    public State Concluido { get; private set; } = null!;
    public State Falhou { get; private set; } = null!;

    public Event<IntegrarCreditoCommand> IntegrarCredito { get; private set; } = null!;
    public Event<ProcessarCreditoCommand> ProcessarCredito { get; private set; } = null!;
    public Event<AuditarCreditoCommand> AuditarCredito { get; private set; } = null!;
    public Event<CreditoProcessadoEvent> CreditoProcessado { get; private set; } = null!;
    public Event<CreditoFalhouEvent> CreditoFalhou { get; private set; } = null!;

    public CreditoSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => IntegrarCredito, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => ProcessarCredito, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => AuditarCredito, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CreditoProcessado, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CreditoFalhou, x => x.CorrelateById(context => context.Message.CorrelationId));

        Initially(
            When(IntegrarCredito)
                .Then(context =>
                {
                    context.Saga.NumeroCredito = context.Message.NumeroCredito;
                    context.Saga.NumeroNfse = context.Message.NumeroNfse;
                    context.Saga.ValorIssqn = context.Message.ValorIssqn;
                    context.Saga.DataIntegracao = DateTime.UtcNow;
                    context.Saga.IntegracaoCompleta = true;
                })
                .TransitionTo(Integrando)
                .Publish(context => new ProcessarCreditoCommand
                {
                    CorrelationId = context.Saga.CorrelationId,
                    NumeroCredito = context.Saga.NumeroCredito,
                    NumeroNfse = context.Saga.NumeroNfse
                })
        );

        During(Integrando,
            When(ProcessarCredito)
                .Then(context =>
                {
                    context.Saga.DataProcessamento = DateTime.UtcNow;
                    context.Saga.TentativasProcessamento++;
                })
                .TransitionTo(Processando)
        );

        During(Processando,
            When(CreditoProcessado)
                .Then(context =>
                {
                    context.Saga.ProcessamentoCompleto = true;
                })
                .TransitionTo(Auditando)
                .Publish(context => new AuditarCreditoCommand
                {
                    CorrelationId = context.Saga.CorrelationId,
                    NumeroCredito = context.Saga.NumeroCredito,
                    Acao = "Crédito processado com sucesso"
                }),
            When(CreditoFalhou)
                .Then(context =>
                {
                    context.Saga.MensagemErro = context.Message.Mensagem;
                })
                .If(context => context.Saga.TentativasProcessamento < 3,
                    binder => binder
                        .Publish(context => new ProcessarCreditoCommand
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            NumeroCredito = context.Saga.NumeroCredito,
                            NumeroNfse = context.Saga.NumeroNfse
                        })
                        .TransitionTo(Processando)
                )
                .If(context => context.Saga.TentativasProcessamento >= 3,
                    binder => binder.TransitionTo(Falhou)
                )
        );

        During(Auditando,
            When(AuditarCredito)
                .Then(context =>
                {
                    context.Saga.AuditoriaCompleta = true;
                    context.Saga.DataConclusao = DateTime.UtcNow;
                })
                .TransitionTo(Concluido)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}

public class IntegrarCreditoCommand
{
    public Guid CorrelationId { get; set; }
    public string NumeroCredito { get; set; } = string.Empty;
    public string NumeroNfse { get; set; } = string.Empty;
    public decimal ValorIssqn { get; set; }
}

public class ProcessarCreditoCommand
{
    public Guid CorrelationId { get; set; }
    public string NumeroCredito { get; set; } = string.Empty;
    public string NumeroNfse { get; set; } = string.Empty;
}

public class AuditarCreditoCommand
{
    public Guid CorrelationId { get; set; }
    public string NumeroCredito { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
}

public class CreditoProcessadoEvent
{
    public Guid CorrelationId { get; set; }
    public string NumeroCredito { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
}

public class CreditoFalhouEvent
{
    public Guid CorrelationId { get; set; }
    public string NumeroCredito { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}
