using CreditoAPI.Application.Commands;
using CreditoAPI.Models;
using CreditoAPI.Repositories;
using MediatR;
using System.Globalization;

namespace CreditoAPI.Application.Handlers
{
    /// <summary>
    /// Handler para processar crédito individual do Service Bus
    /// Implementa Single Responsibility Principle (SOLID)
    /// </summary>
    public class ProcessarCreditoCommandHandler : IRequestHandler<ProcessarCreditoCommand, bool>
    {
        private readonly ICreditoRepository _repository;
        private readonly ILogger<ProcessarCreditoCommandHandler> _logger;

        public ProcessarCreditoCommandHandler(
            ICreditoRepository repository,
            ILogger<ProcessarCreditoCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(ProcessarCreditoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verifica se já existe (idempotência)
                var exists = await _repository.ExistsAsync(request.Credito.NumeroCredito);
                if (exists)
                {
                    _logger.LogWarning("Crédito {NumeroCredito} já existe. Ignorando duplicata.", request.Credito.NumeroCredito);
                    return false;
                }

                // Mapeia DTO para entidade
                var credito = MapToEntity(request.Credito);

                // Persiste no banco
                await _repository.AddAsync(credito);

                _logger.LogInformation("Crédito {NumeroCredito} processado e inserido com sucesso", request.Credito.NumeroCredito);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar crédito {NumeroCredito}", request.Credito.NumeroCredito);
                throw;
            }
        }

        private static Credito MapToEntity(DTOs.CreditoDto dto)
        {
            return new Credito
            {
                NumeroCredito = dto.NumeroCredito,
                NumeroNfse = dto.NumeroNfse,
                DataConstituicao = DateTime.ParseExact(dto.DataConstituicao, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                ValorIssqn = dto.ValorIssqn,
                TipoCredito = dto.TipoCredito,
                SimplesNacional = dto.SimplesNacional.Equals("Sim", StringComparison.OrdinalIgnoreCase),
                Aliquota = dto.Aliquota,
                ValorFaturado = dto.ValorFaturado,
                ValorDeducao = dto.ValorDeducao,
                BaseCalculo = dto.BaseCalculo
            };
        }
    }
}
