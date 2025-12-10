using CreditoAPI.Application.Queries;
using CreditoAPI.DTOs;
using CreditoAPI.Repositories;
using MediatR;

namespace CreditoAPI.Application.Handlers
{
    /// <summary>
    /// Handler para buscar crédito por número
    /// Implementa CQRS - Query Side
    /// </summary>
    public class GetCreditoByNumeroQueryHandler : IRequestHandler<GetCreditoByNumeroQuery, CreditoDto?>
    {
        private readonly ICreditoRepository _repository;
        private readonly ILogger<GetCreditoByNumeroQueryHandler> _logger;

        public GetCreditoByNumeroQueryHandler(
            ICreditoRepository repository,
            ILogger<GetCreditoByNumeroQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<CreditoDto?> Handle(GetCreditoByNumeroQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Buscando crédito por número: {NumeroCredito}", request.NumeroCredito);

                var credito = await _repository.GetByNumeroCreditoAsync(request.NumeroCredito);

                if (credito == null)
                {
                    _logger.LogWarning("Crédito {NumeroCredito} não encontrado", request.NumeroCredito);
                    return null;
                }

                return MapToDto(credito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar crédito {NumeroCredito}", request.NumeroCredito);
                throw;
            }
        }

        private static CreditoDto MapToDto(Models.Credito entity)
        {
            return new CreditoDto
            {
                NumeroCredito = entity.NumeroCredito,
                NumeroNfse = entity.NumeroNfse,
                DataConstituicao = entity.DataConstituicao.ToString("yyyy-MM-dd"),
                ValorIssqn = entity.ValorIssqn,
                TipoCredito = entity.TipoCredito,
                SimplesNacional = entity.SimplesNacional ? "Sim" : "Não",
                Aliquota = entity.Aliquota,
                ValorFaturado = entity.ValorFaturado,
                ValorDeducao = entity.ValorDeducao,
                BaseCalculo = entity.BaseCalculo
            };
        }
    }
}
