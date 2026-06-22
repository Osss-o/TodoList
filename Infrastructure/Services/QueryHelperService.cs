using Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class QueryHelperService : IQueryHelperService
    {
        public Task<List<TSource>> ToListAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return source.ToListAsync(cancellationToken);
        }
    }
}
