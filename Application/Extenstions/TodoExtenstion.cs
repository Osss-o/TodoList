using Application.Dtos.Todo;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application._ُُExtenstions
{
    public static class TodoExtenstion
    {
        public static IQueryable<Todo> ApplyFilters(this IQueryable<Todo> query, TodoFilterDto filter)
        {
            if (filter == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(t=>t.Title.Contains(filter.Title.Trim()));

            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);

            if (filter.RecurrenceType.HasValue)
                query = query.Where(t => t.RecurrenceType == filter.RecurrenceType.Value);

            if (filter.FromeDate.HasValue)
                query = query.Where(t => t.ExpiryDate >= filter.FromeDate.Value.Date);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.ExpiryDate <= filter.ToDate.Value.Date);

            return query;
        }

    }
}