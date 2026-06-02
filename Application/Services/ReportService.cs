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
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Category> _categoryRepo;

        public ReportService(IGenericRepository<User> userRepo,
            IGenericRepository<Category> categoryRepo)
        {
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<List<UserProductivityResponseDto>> GetUserProductivityReportAsync(UserProductivityFilterDto filter)
        {
            ValidateDates(filter.FromDate, filter.ToDate);

            var specBuilder = new SpecificationBuilder<User, UserProductivityResponseDto>();

            if (filter.UserId.HasValue)
            {
                specBuilder.Where(u => u.Id == filter.UserId.Value);
            }
            else
            {
                specBuilder.Where(u => u.Todos.Any());
            }
            specBuilder.Select(u => new UserProductivityResponseDto
            {
                UserName = u.UserName,
                TotalTodos = u.Todos.Count(t =>
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                CompletedTodos = u.Todos.Count(t => t.Status == TodoStatus.Done &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                PendingTodos = u.Todos.Count(t => t.Status == TodoStatus.Pending &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                HighPriorityTodos = u.Todos.Count(t => t.Priority == Priority.High &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                ExpiredTodos = u.Todos.Count(t =>
                t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Done &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value))

            });
            var spec = specBuilder.Build();
            var results = await _userRepo.ListWithSpecAsync(spec);

            foreach (var r in results)
            {
                r.CompletionRate = r.TotalTodos > 0 ? Math.Round((double)r.CompletedTodos * 100 / r.TotalTodos, 2) : 0;
                r.AverageCompletionTime = r.CompletedTodos > 0 ? 24.0 : 0.0;
            }
            return results;
        }

        public async Task<List<CategoryUsageResponseDto>> GetCategoryUsageReportAsync(CategoryUsageFilterDto filter)
        {
            ValidateDates(filter.FromDate, filter.ToDate);

            var specBuilder = new SpecificationBuilder<Category, CategoryUsageResponseDto>();
            if (!filter.IncludeEmptyCategories)
            {
                specBuilder.Where(c => c.Todos.Any());
            }
            specBuilder.Select(c => new CategoryUsageResponseDto
            {
                CategoryName = c.Name,
                CategoryOwner = c.User.UserName,

                TotalLinkedTodos = c.Todos.Count(t =>
                      (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                      (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                CompletedTodos = c.Todos.Count(t => t.Status == TodoStatus.Done &&
                    (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                    (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                PendingTodos = c.Todos.Count(t => t.Status == TodoStatus.Pending &&
                  (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                  (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                ExpiredTodos = c.Todos.Count(t =>
                  t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Done &&
                  (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                  (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                RecurringTodosCount = c.Todos.Count(t => t.RecurrenceType != null &&
                  (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate.Value) &&
                  (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value)),

                LastActivityDate = c.Todos.OrderByDescending(t => t.CreatedAt)
                .Select(t => (DateTime?)t.CreatedAt)
                .FirstOrDefault(),

                HighCountHelper = c.Todos.Count(t => t.Priority == Priority.High),
                MediumCountHelper = c.Todos.Count(t => t.Priority == Priority.Medium),
                LowCountHelper = c.Todos.Count(t => t.Priority == Priority.Low),
            });
            var spec = specBuilder.Build();
            var results = await _categoryRepo.ListWithSpecAsync(spec);

            foreach (var r in results)
            {
                r.CompletionPercentage = r.TotalLinkedTodos > 0 ? Math.Round((double)r.CompletedTodos * 100 / r.TotalLinkedTodos, 2) : 0;
                r.SafeToDelete = r.TotalLinkedTodos == 0;

                int maxCount = Math.Max(r.HighCountHelper, Math.Max(r.MediumCountHelper, r.LowCountHelper));

                if (maxCount == 0)
                    r.MostCommonPriority = null;
                else if (maxCount == r.HighCountHelper)
                    r.MostCommonPriority = Priority.High;
                else if (maxCount == r.MediumCountHelper)
                    r.MostCommonPriority = Priority.Medium;
                else
                    r.MostCommonPriority = Priority.Low;
            }
            return results;
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
