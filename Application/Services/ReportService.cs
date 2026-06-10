using Application.Dtos.Reports;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications;
using Domain.Entities;
using Domain.Entities.Enums;

namespace Application.Services
{
    public class ReportService : IReportService
    {

        private readonly IReportQueryService _reportQueryService;

        public ReportService(IReportQueryService reportQueryService)
        {
            _reportQueryService = reportQueryService;
        }

        public async Task<List<UserProductivityResponseDto>> GetUserProductivityReportAsync(UserProductivityFilterDto filter)
        {
            ValidateDates(filter.FromDate, filter.ToDate);

            var specBuilder = new SpecificationBuilder<User>();
            if (filter.UserId.HasValue)
            {
                specBuilder.Where(u => u.Id == filter.UserId.Value);
            }
            var spec = specBuilder.Build();

            var rawData = await _reportQueryService.GetUserProductivityDataAsync(spec, filter);

            return rawData.Select(u => new UserProductivityResponseDto
            {
                UserName = u.UserName,
                TotalTodos = u.Todos.Count(),
                CompletedTodos = u.Todos.Count(t => t.Status == TodoStatus.Done),
                PendingTodos = u.Todos.Count(t => t.Status == TodoStatus.Pending),
                HighPriorityTodos = u.Todos.Count(t => t.Priority == Priority.High),
                ExpiredTodos = u.Todos.Count(t => t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Pending),

                CompletionRate = u.Todos.Count() == 0 ? 0
                : Math.Round((double)u.Todos.Count(t => t.Status == TodoStatus.Done) / u.Todos.Count() * 100, 2),

                AverageCompletionTime = u.Todos
            .Where(t => t.Status == TodoStatus.Done && t.CompletedAt.HasValue)
            .Select(t => (t.CompletedAt.Value - t.CreatedAt).TotalDays)
            .DefaultIfEmpty(0)
            .Select(avg=>Math.Round(avg,1))
            .Average(),
            }).ToList();
        }
        public async Task<List<CategoryUsageResponseDto>> GetCategoryUsageReportAsync(CategoryUsageFilterDto filter)
        {
            ValidateDates(filter.FromDate, filter.ToDate);
            var specBuilder = new SpecificationBuilder<Category>();
            var spec = specBuilder.Build();

            var rawData = await _reportQueryService.GetCategoryUsagesDataAsync(spec, filter);

            return rawData.Select(c => new CategoryUsageResponseDto
            {
                CategoryName = c.CategoryName,
                CategoryOwner = c.CategoryOwner,
                TotalLinkedTodos = c.Todos.Count(),
                CompletedTodos = c.Todos.Count(t => t.Status == TodoStatus.Done),
                PendingTodos = c.Todos.Count(t => t.Status == TodoStatus.Pending),
                ExpiredTodos = c.Todos.Count(t => t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Pending),
                SafeToDelete = c.Todos.Count() == 0,
                RecurringTodosCount = c.Todos.Count(t => t.RecurrenceType != null),

                CompletionPercentage = c.Todos.Count() == 0 ? 0
              : Math.Round((double)c.Todos.Count(t => t.Status == TodoStatus.Done) /
              c.Todos.Count() * 100, 2),

                LastActivityDate = c.Todos.Select(t => (DateTime?)(t.UpdatedAt ?? t.CreatedAt)).Max(),

                MostCommonPriority = c.Todos
             .GroupBy(t => t.Priority)
             .OrderByDescending(g => g.Count())
             .Select(g => (Priority?)g.Key)
             .FirstOrDefault(),
            }).ToList();
        }
        private void ValidateDates(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue)
            {
                if (fromDate.Value > toDate.Value)
                {
                    throw new ArgumentException("FromDate cannot be later than ToDate.");
                }
            }
            if (fromDate.HasValue && fromDate.Value > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("FromDate cannot be in the future.");
            }
            if (toDate.HasValue && toDate.Value > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("ToDate cannot be in the future.");
            }
        }
    }
}
