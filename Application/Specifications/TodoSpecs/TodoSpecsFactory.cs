using Application.Dtos.Todo;
using Application.Repositories.Interface;
using Domain.Entities;

namespace Application.Specifications.TodoSpecs
{
    public static class TodoSpecsFactory
    {
        public static ISpecification<Todo> GetByIdSpec(int id, int? userId = null)
        {
            var bulider = new SpecificationBuilder<Todo>()
                .Where(t => t.Id == id)
            .Include(t => t.Category)
            .Include(t => t.Attachments)
            .Include(testc => testc.User);

            if (userId.HasValue)
            {
                bulider.Where(t => t.UserId == userId.Value);
            }
            return bulider.Build();
        }
        public static ISpecification<Todo> GetWithFiltersSpec(TodoFilterDto filter, int? userId = null)
        {
            var bulider = new SpecificationBuilder<Todo>();

            bulider.Include(t => t.Category)
              .Include(t => t.Attachments)
              .Include(t => t.User);

            if (userId.HasValue)
            {
                bulider.Where(t => t.UserId == userId.Value);
            }
            if (!string.IsNullOrEmpty(filter.Title))
            {
                bulider.Where(t => t.Title.Contains(filter.Title));
            }
            if (filter.CategoryId.HasValue)
            {
                bulider.Where(t => t.CategoryId == filter.CategoryId.Value);
            }
            if (!string.IsNullOrEmpty(filter.Search))
            {
                bulider.Where(t => t.Title.Contains(filter.Search) || t.Description.Contains(filter.Search));
            }
            if (filter.Status.HasValue)
            {
                bulider.Where(t => t.Status == filter.Status.Value);
            }
            if (filter.Priority.HasValue)
            {
                bulider.Where(t => t.Priority == filter.Priority.Value);
            }
            if (filter.RecurrenceType.HasValue)
            {
                bulider.Where(t => t.RecurrenceType == filter.RecurrenceType.Value);
            }
            if (filter.FromeDate.HasValue)
            {
                bulider.Where(t => t.ExpiryDate >= filter.FromeDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                bulider.Where(t => t.ExpiryDate <= filter.ToDate.Value);
            }
            bulider.OrderBy(t => t.CreatedAt, isDescending: true)
                .ApplyPaging((filter.PageNumber - 1) * filter.PageSize, filter.PageSize);

            return bulider.Build();


        }
    }
}
