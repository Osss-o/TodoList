using Application.Dtos.Reports;
using Application.Repositories.Interface;
using Domain.Entities;
using Domain.Entities.Enums;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class ReportQueryService : IReportQueryService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Category> _categoryRepo;


        public ReportQueryService(IGenericRepository<User> userRepo, IGenericRepository<Category> categoryRepo)
        {
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<List<UserTodosData>> GetUserProductivityDataAsync(ISpecification<User> spec, UserProductivityFilterDto filter)
        {
            var query = _userRepo.GetQuery().AsNoTracking();

            query = SpecificationEvaluator<User>.GetQuery(query, spec);

           return await query.Select(u => new UserTodosData
            {
                UserName = u.UserName,
                Todos = u.Todos.Where(t =>
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate) &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority))
          
                }).ToListAsync();
           
        }

        public async Task<List<CategoryTodosData>> GetCategoryUsagesDataAsync(ISpecification<Category> spec, CategoryUsageFilterDto filter)
        {
            var query = _categoryRepo.GetQuery().AsNoTracking();
            query = SpecificationEvaluator<Category>.GetQuery(query, spec);

           return await query.Select(c => new CategoryTodosData
           {
               CategoryName = c.Name,
               CategoryOwner = c.User.UserName,
               Todos = c.Todos.Where(t =>
               (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate) &&
               (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate))
           })
                .Where (c => filter.IncludeEmptyCategories || c.Todos.Any())
                .ToListAsync();
           
        }

    }
}
