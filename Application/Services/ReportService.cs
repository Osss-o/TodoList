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

        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IQueryHelperService _queryHelperService;


        public ReportService(IGenericRepository<Category> categoryRepo, IGenericRepository<User> userRepo, IQueryHelperService queryHelperService)
        {
            _categoryRepo = categoryRepo;
            _userRepo = userRepo;
            _queryHelperService = queryHelperService;
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

            var query = _userRepo.GetQuery(spec);

            var filteredQuery = query.Select(u => new UserTodosData
            {
                UserName = u.UserName,
                Todos = u.Todos.Where(t =>
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate) &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority))
            });

            var data = await _queryHelperService.ToListAsync(filteredQuery);

            var result= data.Select(u => new UserProductivityResponseDto
            {
                UserName = u.UserName,
                TotalTodos = u.Todos.Count(),
                CompletedTodos = u.Todos.Count(t => t.Status == TodoStatus.Done),
                PendingTodos = u.Todos.Count(t => t.Status == TodoStatus.Pending),
                HighPriorityTodos = u.Todos.Count(t => t.Priority == Priority.High),
                ExpiredTodos = u.Todos.Count(t => t.ExpiryDate < DateTime.UtcNow && t.Status != TodoStatus.Pending),

                CompletionRate = u.Todos.Count() == 0 ? 0
                : Math.Round((double)u.Todos.Count(t => t.Status == TodoStatus.Done) / u.Todos.Count() * 100, 2),

                AverageCompletionTime = Math.Round(u.Todos
                    .Where(t => t.Status == TodoStatus.Done && t.CompletedAt.HasValue)
                    .Select(t => (t.CompletedAt!.Value - t.CreatedAt).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average(),1)
            }).ToList();

            return result;
        }
        public async Task<List<CategoryUsageResponseDto>> GetCategoryUsageReportAsync(CategoryUsageFilterDto filter)
        {
            ValidateDates(filter.FromDate, filter.ToDate);
            var specBuilder = new SpecificationBuilder<Category>();
            var spec = specBuilder.Build();

            var query = _categoryRepo.GetQuery(spec);

            var filteredQuery = query.Select(c => new CategoryTodosData
            {
                CategoryName = c.Name,
                CategoryOwner = c.User.UserName,
                Todos = c.Todos.Where(t =>
                (!filter.FromDate.HasValue || t.CreatedAt >= filter.FromDate) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate))
            })
                .Where(c=> filter.IncludeEmptyCategories || c.Todos.Any());

            var data = await _queryHelperService.ToListAsync(filteredQuery);

            var result = data.Select(c => new CategoryUsageResponseDto
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
             
                LastActivityDate = c.Todos.Max(t => (DateTime?)(t.UpdatedAt ?? t.CreatedAt)),
                
                MostCommonPriority = c.Todos
                    .GroupBy(t => t.Priority)
                    .OrderByDescending(g => g.Count())
                    .Select(g => (Priority?)g.Key)
                    .FirstOrDefault(),
            }).ToList();
           return result;
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
