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

        public async Task<List<UserProductivityResponseDto>> GetUserProductivityAsync(ISpecification<User> spec, UserProductivityFilterDto filter)
        {
            var query = _userRepo.GetQuery().AsNoTracking();

            query = SpecificationEvaluator<User>.GetQuery(query, spec);

            var report = await query.Select(u => new
            {
                u.UserName,
                Todos = u.Todos.Where(t =>
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate) &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority))
            })
                .Select(u => new UserProductivityResponseDto
                {
                    UserName = u.UserName,
                    TotalTodos = u.Todos.Count(),
                    CompletedTodos = u.Todos.Count(t => t.Status == TodoStatus.Done),
                    PendingTodos = u.Todos.Count(t => t.Status == TodoStatus.Pending),
                    HighPriorityTodos = u.Todos.Count(t => t.Priority == Priority.High),
                    ExpiredTodos = u.Todos.Count(t => t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Done),

                    CompletionRate = u.Todos.Count() == 0 ? 0
                    : Math.Round((double)u.Todos.Count(t => t.Status == TodoStatus.Done) / u.Todos.Count() * 100, 2),

                    AverageCompletionTime = u.Todos
                    .Where(t => t.Status == TodoStatus.Done && t.CompletedAt.HasValue)
                    .Select(t => (double?)EF.Functions.DateDiffDay(t.CreatedAt, t.CompletedAt.Value))
                    .Average() ?? 0

                }).ToListAsync();
            return report;
        }

        public async Task<List<CategoryUsageResponseDto>> GetCategoryUsagesAsync(ISpecification<Category> spec, CategoryUsageFilterDto filter)
        {
            var query = _categoryRepo.GetQuery().AsNoTracking();
            query = SpecificationEvaluator<Category>.GetQuery(query, spec);

            var report = await query.Select(c => new
            {
                categoryName = c.Name,
                categoryOwner = c.User.UserName,
                Todos = c.Todos.Where(t =>
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate))
            })
                .Where(c => filter.IncludeEmptyCategories || c.Todos.Any())
                .Select(c => new CategoryUsageResponseDto
                {
                    CategoryName = c.categoryName,
                    CategoryOwner = c.categoryOwner,
                    TotalLinkedTodos = c.Todos.Count(),
                    CompletedTodos = c.Todos.Count(t => t.Status == TodoStatus.Done),
                    PendingTodos = c.Todos.Count(t => t.Status == TodoStatus.Pending),
                    ExpiredTodos = c.Todos.Count(t => t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Done),
                    RecurringTodosCount = c.Todos.Count(t => t.RecurrenceType != null),
                    SafeToDelete = c.Todos.Count() == 0,

                    CompletionPercentage = c.Todos.Count() == 0 ? 0
                    : Math.Round((double)c.Todos.Count(t => t.Status == TodoStatus.Done) / c.Todos.Count() * 100, 2),

                    LastActivityDate = c.Todos.Select(t => (DateTime?)(t.UpdatedAt ?? t.CreatedAt)).Max(),
                    MostCommonPriority = c.Todos
                    .GroupBy(t => t.Priority)
                    .OrderByDescending(g => g.Count())
                    .Select(g => (Priority?)g.Key)
                    .FirstOrDefault()
                })
                .ToListAsync();
            return report;
        }

    }
}
