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
            var bulider = new SpecificationBuilder<Todo>()
                .Where(t =>
                (string.IsNullOrEmpty(filter.Title) || t.Title.Contains(filter.Title)) &&
                (!filter.CategoryId.HasValue || t.CategoryId == filter.CategoryId.Value) &&
                (string.IsNullOrEmpty(filter.Search) || t.Title.Contains(filter.Search)
                || (t.Description != null && t.Description.Contains(filter.Search))) &&
                (!filter.Status.HasValue || t.Status == filter.Status.Value) &&
                (!filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
                (!filter.RecurrenceType.HasValue || t.RecurrenceType == filter.RecurrenceType.Value) &&
                (!filter.FromeDate.HasValue || t.CreatedAt >= filter.FromeDate.Value) &&
                (!filter.ToDate.HasValue || t.CreatedAt <= filter.ToDate.Value))
                .Include(t => t.Category)
                .Include(t => t.Attachments)
                .Include(testc => testc.User);
            if (userId.HasValue)
            {
                bulider.Where(t => t.UserId == userId.Value);
            }
            return bulider.Build();
        }
    }
}
