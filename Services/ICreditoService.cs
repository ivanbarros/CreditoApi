using CreditoAPI.DTOs;

namespace CreditoAPI.Services
{
    public interface ICreditoService
    {
        Task<bool> IntegrarCreditosAsync(List<CreditoDto> creditos);
        Task<List<CreditoDto>> GetByNumeroNfseAsync(string numeroNfse);
        Task<CreditoDto?> GetByNumeroCreditoAsync(string numeroCredito);
        Task ProcessCreditoFromMessageAsync(CreditoDto creditoDto);
    }
}
