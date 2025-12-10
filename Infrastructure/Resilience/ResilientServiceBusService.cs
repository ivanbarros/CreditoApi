using CreditoAPI.DTOs;
using CreditoAPI.Services;
using Polly;

namespace CreditoAPI.Infrastructure.Resilience;

public class ResilientServiceBusService : IServiceBusService
{
    private readonly IServiceBusService _innerService;
    private readonly IAsyncPolicy _policy;
    private readonly ILogger<ResilientServiceBusService> _logger;

    public ResilientServiceBusService(
        IServiceBusService innerService,
        ILogger<ResilientServiceBusService> logger)
    {
        _innerService = innerService;
        _logger = logger;
        _policy = PollyPolicies.CreateCombinedPolicy(logger);
    }

    public async Task SendMessageAsync(CreditoDto credito)
    {
        await _policy.ExecuteAsync(async () =>
        {
            _logger.LogInformation("Sending message with resilience policies for credito {NumeroCredito}", credito.NumeroCredito);
            await _innerService.SendMessageAsync(credito);
        });
    }

    public async Task<List<CreditoDto>> ReceiveMessagesAsync()
    {
        return await _policy.ExecuteAsync(async () =>
        {
            _logger.LogInformation("Receiving messages with resilience policies");
            return await _innerService.ReceiveMessagesAsync();
        });
    }

    public async Task SendAuditMessageAsync(string eventType, string key)
    {
        await _policy.ExecuteAsync(async () =>
        {
            _logger.LogInformation("Sending audit message with resilience policies: {EventType}", eventType);
            await _innerService.SendAuditMessageAsync(eventType, key);
        });
    }
}
