using System.Linq.Expressions;

namespace Application.Repositories.Interface
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T,bool>>>Criterias { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        bool IsDescending { get; }
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
    public interface ISpecification<T, TResult> : ISpecification<T>
    {
        Expression<Func<T, TResult>>? Selector { get; }
        Expression<Func<T, object>>? GroupBy { get; }
    }
}
