using System.Linq.Expressions;

namespace CreditoAPI.Infrastructure.Specifications
{
    /// <summary>
    /// Interface base para Specification Pattern
    /// Permite encapsular lógica de consulta reutilizável
    /// Implementa SOLID - Open/Closed Principle
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
}
