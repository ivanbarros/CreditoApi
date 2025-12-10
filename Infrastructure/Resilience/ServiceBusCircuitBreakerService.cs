using CreditoAPI.DTOs;
using CreditoAPI.Services;
using Polly;
using Polly.CircuitBreaker;

namespace CreditoAPI.Infrastructure.Resilience
{
    public class ServiceBusCircuitBreakerService : IServiceBusService
    {
        private readonly IServiceBusService _innerService;
        private readonly ILogger<ServiceBusCircuitBreakerService> _logger;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public ServiceBusCircuitBreakerService(
            IServiceBusService innerService, 
            ILogger<ServiceBusCircuitBreakerService> logger)
        {
            _innerService = innerService;
            _logger = logger;

            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        _logger.LogWarning("Circuit breaker opened for {Duration} seconds due to: {Exception}", 
                            duration.TotalSeconds, exception.Message);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit breaker half-open, testing connection");
                    }
                );
        }

        public async Task SendMessageAsync(CreditoDto credito)
        {
            try
            {
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    await _innerService.SendMessageAsync(credito);
                });
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is open. Message not sent for credito: {NumeroCredito}", 
                    credito.NumeroCredito);
                throw new InvalidOperationException("Service Bus is temporarily unavailable");
            }
        }

        public async Task<List<CreditoDto>> ReceiveMessagesAsync()
        {
            try
            {
                return await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    return await _innerService.ReceiveMessagesAsync();
                });
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is open. Cannot receive messages");
                return new List<CreditoDto>();
            }
        }

        public async Task SendAuditMessageAsync(string eventType, string key)
        {
            try
            {
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    await _innerService.SendAuditMessageAsync(eventType, key);
                });
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is open. Audit message not sent: {EventType} - {Key}", 
                    eventType, key);
            }
        }
    }
}
