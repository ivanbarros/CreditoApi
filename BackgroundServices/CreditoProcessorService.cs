using CreditoAPI.Application.Commands;
using CreditoAPI.Services;
using MediatR;

namespace CreditoAPI.BackgroundServices
{
    /// <summary>
    /// Background Service para processar créditos do Service Bus
    /// Implementa CQRS usando MediatR para enviar comandos
    /// </summary>
    public class CreditoProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CreditoProcessorService> _logger;
        private readonly int _processingIntervalMs;

        public CreditoProcessorService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<CreditoProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _processingIntervalMs = configuration.GetValue<int>("BackgroundService:ProcessingIntervalMs", 500);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Credito Processor Service started. Processing interval: {IntervalMs}ms", _processingIntervalMs);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var serviceBusService = scope.ServiceProvider.GetRequiredService<IServiceBusService>();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        var messages = await serviceBusService.ReceiveMessagesAsync();

                        if (messages.Any())
                        {
                            _logger.LogInformation("Received {Count} messages from Service Bus", messages.Count);

                            foreach (var creditoDto in messages)
                            {
                                try
                                {
                                    // CQRS - Envia comando via MediatR
                                    var command = new ProcessarCreditoCommand(creditoDto);
                                    await mediator.Send(command);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error processing credito: {NumeroCredito}", creditoDto.NumeroCredito);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Credito Processor Service");
                }

                await Task.Delay(_processingIntervalMs, stoppingToken);
            }

            _logger.LogInformation("Credito Processor Service stopped");
        }
    }
}
