using CreditoAPI.DTOs;
using CreditoAPI.Models;
using CreditoAPI.Repositories;
using System.Globalization;

namespace CreditoAPI.Services
{
    public class CreditoService : ICreditoService
    {
        private readonly ICreditoRepository _repository;
        private readonly IServiceBusService _serviceBusService;
        private readonly ILogger<CreditoService> _logger;

        public CreditoService(
            ICreditoRepository repository,
            IServiceBusService serviceBusService,
            ILogger<CreditoService> logger)
        {
            _repository = repository;
            _serviceBusService = serviceBusService;
            _logger = logger;
        }

        public async Task<bool> IntegrarCreditosAsync(List<CreditoDto> creditos)
        {
            try
            {
                foreach (var creditoDto in creditos)
                {
                    await _serviceBusService.SendMessageAsync(creditoDto);
                    _logger.LogInformation("Credito sent to Service Bus: {NumeroCredito}", creditoDto.NumeroCredito);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error integrating creditos");
                throw;
            }
        }

        public async Task<List<CreditoDto>> GetByNumeroNfseAsync(string numeroNfse)
        {
            try
            {
                var creditos = await _repository.GetByNumeroNfseAsync(numeroNfse);
                return creditos.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving creditos by numero nfse: {NumeroNfse}", numeroNfse);
                throw;
            }
        }

        public async Task<CreditoDto?> GetByNumeroCreditoAsync(string numeroCredito)
        {
            try
            {
                var credito = await _repository.GetByNumeroCreditoAsync(numeroCredito);
                return credito != null ? MapToDto(credito) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credito by numero credito: {NumeroCredito}", numeroCredito);
                throw;
            }
        }

        public async Task ProcessCreditoFromMessageAsync(CreditoDto creditoDto)
        {
            try
            {
                // Check if credito already exists
                var exists = await _repository.ExistsAsync(creditoDto.NumeroCredito);
                if (exists)
                {
                    _logger.LogWarning("Credito already exists: {NumeroCredito}. Skipping insertion.", creditoDto.NumeroCredito);
                    return;
                }

                var credito = MapToEntity(creditoDto);
                await _repository.AddAsync(credito);
                _logger.LogInformation("Credito inserted into database: {NumeroCredito}", creditoDto.NumeroCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing credito from message: {NumeroCredito}", creditoDto.NumeroCredito);
                throw;
            }
        }

        private static Credito MapToEntity(CreditoDto dto)
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

        private static CreditoDto MapToDto(Credito entity)
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
