using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface IQueryHelperService
    {
        Task<List<TSource>> ToListAsync<TSource>( IQueryable<TSource> source, CancellationToken cancellationToken = default);
    }
}
