using CreditoAPI.Data;
using CreditoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditoAPI.Repositories
{
    public class CreditoRepository : ICreditoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreditoRepository> _logger;

        public CreditoRepository(ApplicationDbContext context, ILogger<CreditoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Credito?> GetByNumeroCreditoAsync(string numeroCredito)
        {
            try
            {
                return await _context.Creditos
                    .FirstOrDefaultAsync(c => c.NumeroCredito == numeroCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credito by numero credito: {NumeroCredito}", numeroCredito);
                throw;
            }
        }

        public async Task<List<Credito>> GetByNumeroNfseAsync(string numeroNfse)
        {
            try
            {
                return await _context.Creditos
                    .Where(c => c.NumeroNfse == numeroNfse)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving creditos by numero nfse: {NumeroNfse}", numeroNfse);
                throw;
            }
        }

        public async Task<Credito> AddAsync(Credito credito)
        {
            try
            {
                await _context.Creditos.AddAsync(credito);
                await _context.SaveChangesAsync();
                return credito;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding credito: {NumeroCredito}", credito.NumeroCredito);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string numeroCredito)
        {
            try
            {
                return await _context.Creditos
                    .AnyAsync(c => c.NumeroCredito == numeroCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if credito exists: {NumeroCredito}", numeroCredito);
                throw;
            }
        }
    }
}
