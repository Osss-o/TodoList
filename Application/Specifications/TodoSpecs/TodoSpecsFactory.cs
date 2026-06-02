using Application.Dtos.Todo;
using Application.Repositories.Interface;
using Domain.Entities;

namespace Application.Specifications.TodoSpecs
{
    public static class TodoSpecsFactory
    {
        public static ISpecification<Todo> GetByIdSpec(int id, int? userId = null)
        {
            var builder = new SpecificationBuilder<Todo>()
                .Where(t => t.Id == id)
            .Include(t => t.Category)
            .Include(t => t.Attachments)
            .Include(t => t.User);

            if (userId.HasValue)
            {
                builder.Where(t => t.UserId == userId.Value);
            }
            return builder.Build();
        }
        public static ISpecification<Todo> GetWithFiltersSpec(TodoFilterDto filter, int? userId = null)
        {
            var builder = new SpecificationBuilder<Todo>();

            builder.Include(t => t.Category)
              .Include(t => t.Attachments)
              .Include(t => t.User);

            if (userId.HasValue)
            {
                builder.Where(t => t.UserId == userId.Value);
            }
            if (!string.IsNullOrEmpty(filter.Title))
            {
                builder.Where(t => t.Title.Contains(filter.Title));
            }
            if (!string.IsNullOrEmpty(filter.UserName))
            {
                builder.Where(t => t.User != null && t.User.UserName == (filter.UserName.Trim()));
            }
            if (filter.CategoryId.HasValue)
            {
                builder.Where(t => t.CategoryId == filter.CategoryId.Value);
            }
            if (!string.IsNullOrEmpty(filter.Search))
            {
                builder.Where(t => t.Title.Contains(filter.Search) || t.Description.Contains(filter.Search));
            }
            if (filter.Status.HasValue)
            {
                builder.Where(t => t.Status == filter.Status.Value);
            }
            if (filter.Priority.HasValue)
            {
                builder.Where(t => t.Priority == filter.Priority.Value);
            }
            if (filter.RecurrenceType.HasValue)
            {
                builder.Where(t => t.RecurrenceType == filter.RecurrenceType.Value);
            }
            if (filter.FromeDate.HasValue)
            {
                builder.Where(t => t.ExpiryDate >= filter.FromeDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                builder.Where(t => t.ExpiryDate <= filter.ToDate.Value);
            }
            builder.OrderBy(t => t.CreatedAt, isDescending: true)
                .ApplyPaging((filter.PageNumber - 1) * filter.PageSize, filter.PageSize);

            return builder.Build();


        }
    }
}
