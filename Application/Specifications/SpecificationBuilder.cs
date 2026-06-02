using Application.Repositories.Interface;
using Application.SpecificationsServices;
using System.Linq.Expressions;

namespace Application.Specifications
{
    public class SpecificationBuilder<T>
    {
        private readonly Specification<T> _specification = new Specification<T>();

        public SpecificationBuilder<T> Where(Expression<Func<T, bool>> criteria)
        {
            _specification.Criterias.Add(criteria);
            return this;
        }
        public SpecificationBuilder<T> Include(Expression<Func<T, object>> includeExpression)
        {
            _specification.Includes.Add(includeExpression);
            return this;
        }
        public SpecificationBuilder<T> OrderBy(Expression<Func<T, object>> orderByExpression, bool isDescending = false)
        {
            _specification.OrderBy = orderByExpression;
            _specification.IsDescending = isDescending;
            return this;
        }
        public SpecificationBuilder<T> ApplyPaging(int skip, int take)
        {
            _specification.Skip = skip;
            _specification.Take = take;
            _specification.IsPagingEnabled = true;
            return this;
        }
        public ISpecification<T> Build()
        {
            return _specification;
        }

    }
    public class SpecificationBuilder<T, TResult>

    {
        private readonly Specification<T, TResult> _specification = new Specification<T, TResult>();
       
        public SpecificationBuilder<T,TResult>Where(Expression<Func<T, bool>> criteria)
        {
            _specification.Criterias.Add(criteria);
            return this;
        }
        public SpecificationBuilder<T,TResult> Include(Expression<Func<T, object>> includeExpression)
        {
            _specification.Includes.Add(includeExpression);
            return this;
        }
        public SpecificationBuilder<T, TResult> OrderBy(Expression<Func<T, object>> orderByExpression, bool isDescending = false)
        {
            _specification.OrderBy = orderByExpression;
            _specification.IsDescending = isDescending;
            return this;
        }
        public SpecificationBuilder<T,TResult> ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            _specification.GroupBy = groupByExpression;
            return this;
        }
        public SpecificationBuilder<T,TResult> Select(Expression<Func<T, TResult>> selector)
        {
            _specification.Selector = selector;
            return this;
        }
        public ISpecification<T,TResult>Build()
        {
            return _specification;
        }
    }
}
