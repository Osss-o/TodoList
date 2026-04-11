using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetById(int id);
        IQueryable<T> GetAll();
        Task Insert(T entity);
        Task InsertRange(List<T> entities);
        void Update(T entity);
        Task Delete(T entity);
        Task DeleteRange(List<T> entities);
        Task<int> SaveChanges();

    }
}
