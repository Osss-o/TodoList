using Application.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.SpecificationsServices
{
    public class Specification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>> Criteria { get; internal set; }
        public List<Expression<Func<T, bool>>> Criterias { get; } = new List<Expression<Func<T, bool>>>();

        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        public Expression<Func<T, object>> OrderBy { get; internal set; }
        public bool IsDescending { get; internal set; }
        public int Take { get; internal set; }
        public int Skip { get; internal set; }
        public bool IsPagingEnabled { get; internal set; }
    }
}
