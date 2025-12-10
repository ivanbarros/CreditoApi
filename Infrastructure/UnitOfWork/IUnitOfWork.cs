using CreditoAPI.Repositories;

namespace CreditoAPI.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Interface do Unit of Work Pattern
    /// Gerencia transações e coordena múltiplos repositórios
    /// Implementa SOLID - Interface Segregation Principle
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ICreditoRepository Creditos { get; }
        Task<int> CommitAsync();
        Task RollbackAsync();
    }
}
