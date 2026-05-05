using Application.Dtos.Todo;
using Domain.Entities;

namespace Application.Specifications.TodoSpecs
{
    public class TodoWithFiltersSpecs : BaseSpecification<Todo>
    {
        public TodoWithFiltersSpecs(TodoFilterDto filter, int userId, bool isAdmin)

            : base(x => 
               (isAdmin || x.UserId == userId) &&

                (string.IsNullOrEmpty(filter.Search) || x.Title.Contains(filter.Search)) &&

                (!filter.CategoryId.HasValue || x.CategoryId == filter.CategoryId) &&
                (!filter.Priority.HasValue || x.Priority == filter.Priority) &&
                (!filter.Status.HasValue || x.Status == filter.Status) &&
                (!filter.RecurrenceType.HasValue || x.RecurrenceType == filter.RecurrenceType))
                
        {
            AddInclude(x => x.Category);
            AddInclude(x => x.User);
            AddInclude(x => x.Attachments);

            ApplyOrderByDescending(x => x.CreatedAt);

            ApplyPaging((filter.PageNumber - 1) * filter.PageSize, filter.PageSize);
        }

    }
}
