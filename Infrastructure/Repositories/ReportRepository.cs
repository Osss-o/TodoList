using Application.Dtos.Reports;
using Application.Repositories.Interface;
using Domain.Entities.Enums;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly TodoListDbContext _context;

        public ReportRepository(TodoListDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserProductivityResponseDto>> GetUserProductivityResponsesAsync(UserProductivityFilterDto filter)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (filter.UserId.HasValue)
            {
                query = query.Where(u => u.Id == filter.UserId.Value);
            }
            var report = await query.Select(u => new
            {
                u.UserName,
                Todos = u.Todos.Where(t =>
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)
                )
            })
                .Where(u => u.Todos.Any() || filter.UserId.HasValue)
                .Select(u => new UserProductivityResponseDto
                {
                    UserName = u.UserName,
                    TotalTodos = u.Todos.Count(),
                    CompletedTodos = u.Todos.Count(t => t.Status == TodoStatus.Done),
                    PendingTodos = u.Todos.Count(t => t.Status == TodoStatus.Pending),
                    HighPriorityTodos = u.Todos.Count(t => t.Priority == Priority.High),
                    ExpiredTodos = u.Todos.Count(t => t.ExpiryDate < DateTime.UtcNow &&
                    t.Status != TodoStatus.Done),

                    CompletionRate = u.Todos.Count() == 0 ? 0
                    : Math.Round((double)u.Todos.Count(t => t.Status == TodoStatus.Done) * 100 / u.Todos.Count(), 2),

                    AverageCompletionTime = u.Todos
                    .Where(t => t.Status == TodoStatus.Done && t.CompletedAt.HasValue)
                    .Select(t => (double?)EF.Functions.DateDiffHour(t.CreatedAt, t.CompletedAt.Value))
                    .Average() ?? 0

                }).ToListAsync();
            return report;
        }

        public async Task<List<CategoryUsageResponseDto>> GetCategoryUsagesAsync(CategoryUsageFilterDto filter)

        {
            var query = _context.Category.AsNoTracking().AsQueryable();
            var report = await query.Select(c => new
            {
                CategoryName = c.Name,
                CategoryOwner = c.User.UserName,
                Todos = c.Todos.Where(t =>
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)
                )
            })
                .Where(c => filter.IncludeEmptyCategories || c.Todos.Any())
                .Select(c => new CategoryUsageResponseDto
                {
                    CategoryName = c.CategoryName,
                    CategoryOwner = c.CategoryOwner,
                    TotalLinkedTodos = c.Todos.Count(),
                    CompletedTodos = c.Todos.Count(t => t.Status == TodoStatus.Done),
                    PendingTodos = c.Todos.Count(t => t.Status == TodoStatus.Pending),
                    ExpiredTodos = c.Todos.Count
                    (t => t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Done),
                    RecurringTodosCount = c.Todos.Count(t => t.RecurrenceType != null),
                    SafeToDelete = c.Todos.Count() == 0,

                    CompletionPercentage = c.Todos.Count() == 0
                    ? 0 : Math.Round((double)c.Todos.Count(t => t.Status == TodoStatus.Done) * 100 / c.Todos.Count(), 2),

                    LastActivityDate = c.Todos.Select(t => (DateTime?)(t.UpdatedAt ?? t.CreatedAt)).Max(),

                    MostCommonPriority = c.Todos
                    .GroupBy(t => t.Priority)
                    .OrderByDescending(g => g.Count())
                    .Select(g => (Priority?)g.Key)
                    .FirstOrDefault()

                }).ToListAsync();
            return report;

        }
    }
}
