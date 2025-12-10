using CreditoAPI.Application.Commands;
using CreditoAPI.DTOs;
using CreditoAPI.Services;
using MediatR;

namespace CreditoAPI.Application.Handlers
{
    /// <summary>
    /// Handler para processar o comando de integração de créditos
    /// Implementa Single Responsibility Principle (SOLID)
    /// </summary>
    public class IntegrarCreditosCommandHandler : IRequestHandler<IntegrarCreditosCommand, IntegrarCreditoResponse>
    {
        private readonly IServiceBusService _serviceBusService;
        private readonly ILogger<IntegrarCreditosCommandHandler> _logger;

        public IntegrarCreditosCommandHandler(
            IServiceBusService serviceBusService,
            ILogger<IntegrarCreditosCommandHandler> logger)
        {
            _serviceBusService = serviceBusService;
            _logger = logger;
        }

        public async Task<IntegrarCreditoResponse> Handle(IntegrarCreditosCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Iniciando integração de {Count} créditos", request.Creditos.Count);

                foreach (var credito in request.Creditos)
                {
                    await _serviceBusService.SendMessageAsync(credito);
                    _logger.LogInformation("Crédito {NumeroCredito} enviado ao Service Bus", credito.NumeroCredito);
                }

                _logger.LogInformation("Integração concluída com sucesso");
                return new IntegrarCreditoResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao integrar créditos");
                throw;
            }
        }
    }
}
