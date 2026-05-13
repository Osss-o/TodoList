using Application.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SpecificationEvaluator<TEntity> where TEntity : class
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
        {
            var query = inputQuery;

            if (spec.Criterias != null && spec.Criterias.Any())
            {
                foreach (var criteria in spec.Criterias)
                {
                    query = query.Where(criteria);
                }
            }
          
           if (spec.OrderBy != null)
            {
                if(spec.IsDescending)
                {
                    query = query.OrderByDescending(spec.OrderBy);
                }
                else
                {
                    query = query.OrderBy(spec.OrderBy);
                }
            }
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            if (spec.Includes != null && spec.Includes.Any())
            {
                query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return query;
        }
    }
}
