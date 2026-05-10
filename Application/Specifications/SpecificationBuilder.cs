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
}
