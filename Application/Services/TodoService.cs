using Application._ُُExtenstions;
using Application.Dtos.PagedResult;
using Application.Dtos.Todo;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TodoService : ITodoService
    {
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly IGenericRepository<Category> _categoryRepo;

        public TodoService(
            IGenericRepository<Todo> todoRepo,
            IGenericRepository<Category> categoryRepo)

        {
            _todoRepo = todoRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task CreateAsync(TodoCreateDto todo, int userId)
        {
            if (todo.DueDate.HasValue && todo.DueDate.Value.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("Its due date cannot be determined in the past.");

            if (todo.CategoryId.HasValue)
            {
                var categoryExixts = await _categoryRepo.GetById(todo.CategoryId.Value);
                if (categoryExixts == null)
                    throw new KeyNotFoundException("Category not found.");
            }
            var todoObj = new Todo
            {
                Title = todo.Title.Trim(),
                Description = todo.Description?.Trim(),
                ExpiryDate = todo.DueDate ?? DateTime.UtcNow.Date,
                CategoryId = todo.CategoryId,
                Priority = todo.Priority,
                RecurrenceType = todo.RecurrenceType,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Status = TodoStatus.Pending
            };
            await _todoRepo.Insert(todoObj);
            await _todoRepo.SaveChanges();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var todo = _todoRepo.GetAll()
                .FirstOrDefault(x => x.Id == id && x.UserId == userId);

            if (todo == null)
                throw new KeyNotFoundException("Todo not found.");


            _todoRepo.Delete(todo);
            await _todoRepo.SaveChanges();

        }

        public async Task<List<TodoListDto>> GetAllAsync(TodoFilterDto filter, int userId)
        {
           return await _todoRepo.GetAll()
               .AsNoTracking()
               .Where(x => x.UserId == userId)
               .Include(x => x.Category)
               .ApplyFilters(filter)
               .OrderByDescending(x => x.CreatedAt)
              
                .Select(x => new TodoListDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    DueDate = x.ExpiryDate,
                    Status = x.Status,
                    Priority = x.Priority,
                    RecurrenceType = x.RecurrenceType
                }).ToListAsync();
        }


        public async Task<TodoListDto?> GetByIdAsync(int id, int userId)
        {

            var todo = await _todoRepo.GetAll()
                .Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (todo == null) return null;

            return new TodoListDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                CategoryName = todo.Category != null ? todo.Category.Name : null,
                DueDate = todo.ExpiryDate,
                Status = todo.Status,
                Priority = todo.Priority,
                RecurrenceType = todo.RecurrenceType
            };

        }

        public async Task<PagedResultDto<TodoListDto>> SearchAsync(TodoFilterDto filter, int userId, bool isAdmin = false)
        {
            var query = _todoRepo.GetAll()
                 .Include(t => t.Category)
                  .AsNoTracking();

            if (!isAdmin)
                query = query.Where(x => x.UserId == userId);

            query= query.ApplyFilters(filter);

            var totalCount = await query.CountAsync();

            var todos = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new TodoListDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    DueDate = x.ExpiryDate,
                    Status = x.Status,
                    Priority = x.Priority,
                    RecurrenceType= x.RecurrenceType,
                })
                .ToListAsync();

            return new PagedResultDto<TodoListDto>
            {
                Results = todos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task UpdateAsync(int id, TodoUpdateDto todo, int userId)
        {
            var todoObj = await _todoRepo.GetAll()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (todoObj == null)
                throw new KeyNotFoundException("Todo not found.");

            if (!string.IsNullOrWhiteSpace(todo.Title))
                todoObj.Title = todo.Title.Trim();

            if (todo.Description != null)
                todo.Description = todo.Description.Trim();

            if (todo.CategoryId.HasValue)
            {
                var categoryExixts = await _categoryRepo.GetById(todo.CategoryId.Value);

                if (categoryExixts == null)

                    throw new KeyNotFoundException("Category not found.");

                todoObj.CategoryId = todo.CategoryId;
            }
            if (todo.DueDate.HasValue)
            {
                if (todo.DueDate.Value.Date < DateTime.UtcNow.Date)

                    throw new InvalidOperationException("Its due date cannot be determined in the past.");

                todoObj.ExpiryDate = todo.DueDate.Value.Date;
            }
            if (todo.Status.HasValue) todoObj.Status = todo.Status.Value;
            if (todo.Priority.HasValue) todoObj.Priority = todo.Priority.Value;
            if (todo.RecurrenceType.HasValue) todoObj.RecurrenceType= todo.RecurrenceType.Value;
      
            _todoRepo.Update(todoObj);
            await _todoRepo.SaveChanges();
        }

    }


}
