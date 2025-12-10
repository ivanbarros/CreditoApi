using CreditoAPI.Models;

namespace CreditoAPI.Repositories
{
    public interface ICreditoRepository
    {
        Task<Credito?> GetByNumeroCreditoAsync(string numeroCredito);
        Task<List<Credito>> GetByNumeroNfseAsync(string numeroNfse);
        Task<Credito> AddAsync(Credito credito);
        Task<bool> ExistsAsync(string numeroCredito);
    }
}
