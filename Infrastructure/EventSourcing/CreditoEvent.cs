namespace CreditoAPI.Infrastructure.EventSourcing;

public abstract class CreditoEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
}

public class CreditoIntegradoEvent : CreditoEvent
{
    public string NumeroCredito { get; set; } = string.Empty;
    public string NumeroNfse { get; set; } = string.Empty;
    public decimal ValorIssqn { get; set; }
    public string TipoCredito { get; set; } = string.Empty;
    public bool SimplesNacional { get; set; }
    public decimal Aliquota { get; set; }
    public decimal ValorFaturado { get; set; }
    public decimal ValorDeducao { get; set; }
    public decimal BaseCalculo { get; set; }
    public DateTime DataConstituicao { get; set; }

    public CreditoIntegradoEvent()
    {
        EventType = nameof(CreditoIntegradoEvent);
    }
}

public class CreditoProcessadoEvent : CreditoEvent
{
    public string NumeroCredito { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }

    public CreditoProcessadoEvent()
    {
        EventType = nameof(CreditoProcessadoEvent);
    }
}

public class CreditoConsultadoEvent : CreditoEvent
{
    public string? NumeroCredito { get; set; }
    public string? NumeroNfse { get; set; }
    public int QuantidadeResultados { get; set; }

    public CreditoConsultadoEvent()
    {
        EventType = nameof(CreditoConsultadoEvent);
    }
}
