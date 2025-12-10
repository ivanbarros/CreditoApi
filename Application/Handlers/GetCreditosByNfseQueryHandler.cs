using CreditoAPI.Application.Queries;
using CreditoAPI.DTOs;
using CreditoAPI.Repositories;
using MediatR;

namespace CreditoAPI.Application.Handlers
{
    /// <summary>
    /// Handler para buscar créditos por NFS-e
    /// Implementa CQRS - Query Side
    /// </summary>
    public class GetCreditosByNfseQueryHandler : IRequestHandler<GetCreditosByNfseQuery, List<CreditoDto>>
    {
        private readonly ICreditoRepository _repository;
        private readonly ILogger<GetCreditosByNfseQueryHandler> _logger;

        public GetCreditosByNfseQueryHandler(
            ICreditoRepository repository,
            ILogger<GetCreditosByNfseQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<CreditoDto>> Handle(GetCreditosByNfseQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Buscando créditos por NFS-e: {NumeroNfse}", request.NumeroNfse);

                var creditos = await _repository.GetByNumeroNfseAsync(request.NumeroNfse);

                _logger.LogInformation("Encontrados {Count} créditos para NFS-e {NumeroNfse}", 
                    creditos.Count, request.NumeroNfse);

                return creditos.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar créditos por NFS-e {NumeroNfse}", request.NumeroNfse);
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
