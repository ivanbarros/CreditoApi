using CreditoAPI.Data;
using CreditoAPI.Repositories;

namespace CreditoAPI.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Implementação do Unit of Work Pattern
    /// Coordena operações de múltiplos repositórios em uma única transação
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILogger<CreditoRepository> _repositoryLogger;
        private ICreditoRepository? _creditoRepository;
        private bool _disposed = false;

        public UnitOfWork(
            ApplicationDbContext context, 
            ILogger<UnitOfWork> logger,
            ILogger<CreditoRepository> repositoryLogger)
        {
            _context = context;
            _logger = logger;
            _repositoryLogger = repositoryLogger;
        }

        public ICreditoRepository Creditos
        {
            get
            {
                if (_creditoRepository == null)
                {
                    _creditoRepository = new CreditoRepository(_context, _repositoryLogger);
                }
                return _creditoRepository;
            }
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Transação commitada com sucesso. {Count} registros afetados", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao commitar transação");
                throw;
            }
        }

        public Task RollbackAsync()
        {
            _logger.LogWarning("Rollback executado");
            // Entity Framework não mantém mudanças até SaveChanges ser chamado
            // Então apenas descartamos o contexto
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
