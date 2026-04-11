using Application.Repositories.Interface;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly TodoListDbContext _todolistDbcontext;
        

        public GenericRepository(TodoListDbContext todolistDbcontext)
        {
            _todolistDbcontext = todolistDbcontext;
            
        }
        public async Task <T?> GetById(int id)
        {
            return await _todolistDbcontext.Set<T>().FindAsync(id);
        }
        public IQueryable<T> GetAll()
        {
            return _todolistDbcontext.Set<T>();
        }
        public async Task Insert(T entity)
        {
            await _todolistDbcontext.Set<T>().AddAsync(entity);
        }
        public async Task InsertRange(List<T> entities)
        {
            await _todolistDbcontext.Set<T>().AddRangeAsync(entities);
        }
        public void Update(T entity)
        {
            _todolistDbcontext.Set<T>().Update(entity);
        }
        public async Task Delete(T entity)
        {
            _todolistDbcontext.Set<T>().Remove(entity);
           
        }
        public async Task DeleteRange(List<T> entities)
            {
            _todolistDbcontext.Set<T>().RemoveRange(entities);
           
        }
        public async Task<int> SaveChanges()
        {
            return await _todolistDbcontext.SaveChangesAsync();
        }

    }
}
