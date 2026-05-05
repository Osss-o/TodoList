using Application._ُُExtenstions;
using Application.Dtos.FileAttachment;
using Application.Dtos.PagedResult;
using Application.Dtos.Todo;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications.TodoSpecs;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TodoService : ITodoService
    {
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IFileAttachmentService _fileService;
        public TodoService(
            IGenericRepository<Todo> todoRepo,
            IGenericRepository<Category> categoryRepo,
            IFileAttachmentService fileService)

        {
            _todoRepo = todoRepo;
            _categoryRepo = categoryRepo;
            _fileService = fileService;
        }

        public async Task CreateAsync(TodoCreateDto todo, int userId)
        {
            if (todo.DueDate.HasValue && todo.DueDate.Value.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("Its due date cannot be determined in the past.");

            if (todo.CategoryId.HasValue)
            {
                var categoryExixts = await _categoryRepo.GetById(todo.CategoryId.Value);

                if (categoryExixts == null || categoryExixts.UserId != userId)
                    throw new KeyNotFoundException("Category not found or access denied.");
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

            if (todo.File != null && todo.File.Length > 0)
            {
                await _fileService.CreateAsync(new FileAttachmentCreateDto
                {
                    TodoId = todoObj.Id,
                    File = todo.File
                }, userId);
            }
            if (todo.Files != null && todo.Files.Any())
            {
                await _fileService.CreateManyAsync(todo.Files, todoObj.Id, userId);
            }

        }

        public async Task DeleteAsync(int id, int userId, bool isAdmin = false)
        {
            var spec = new TodoByIdSpecs(id, userId, isAdmin);
            var todo = await _todoRepo.GetEntityWithSpec(spec);

            if (todo == null)
                throw new KeyNotFoundException("Todo not found.");

            foreach (var file in todo.Attachments)
            {
                await _fileService.DeleteAsync(file.Id, userId, isAdmin);
            }

            await _todoRepo.Delete(todo);
            await _todoRepo.SaveChanges();

        }

        public async Task<List<TodoListDto>> GetAllAsync(TodoFilterDto filter, int userId)
        { 
            filter.PageNumber = 1;
            filter.PageSize = int.MaxValue;

            var spec = new TodoWithFiltersSpecs(filter, userId,isAdmin: false);
            var todos = await _todoRepo.ListWithSpecAsync(spec);
             
            return todos.Select(x => new TodoListDto
                 {
                     Id = x.Id,
                     Title = x.Title,
                     UserName = x.User?.UserName,
                     Description = x.Description,
                     CategoryName = x.Category?.Name,
                     DueDate = x.ExpiryDate,
                     Status = x.Status,
                     Priority = x.Priority,
                     RecurrenceType = x.RecurrenceType,
                     CreatedAt =x.CreatedAt,
                     UpdatedAt = x.UpdatedAt,
                     HasAttachments = x.Attachments.Any()
                 }).ToList();
        }


        public async Task<TodoListDto?> GetByIdAsync(int id, int userId, bool isAdmin = false)
        {

            var spec = new TodoByIdSpecs(id, userId, isAdmin);
            var todo = await _todoRepo.GetEntityWithSpec(spec);

            if (todo == null) return null;

            return new TodoListDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                CategoryName = todo.Category?.Name,
                UserName = todo.User?.UserName,
                DueDate = todo.ExpiryDate,
                Status = todo.Status,
                Priority = todo.Priority,
                RecurrenceType = todo.RecurrenceType,
                CreatedAt=todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt,
                HasAttachments = todo.Attachments.Any()

            };

        }

        public async Task<PagedResultDto<TodoListDto>> SearchAsync(TodoFilterDto filter, int userId, bool isAdmin = false)
        {
            var spec = new TodoWithFiltersSpecs(filter, userId, isAdmin);

            var todos = await _todoRepo.ListWithSpecAsync(spec);

            var countFilter = new TodoFilterDto
            {
                Search = filter.Search,
                CategoryId = filter.CategoryId,
                Status = filter.Status,
                Priority = filter.Priority,
                RecurrenceType = filter.RecurrenceType,
                Title = filter.Title,
                FromeDate = filter.FromeDate,
                ToDate = filter.ToDate,
                PageNumber =1,
                PageSize = int.MaxValue
            };

            var countSpec = new TodoWithFiltersSpecs(countFilter, userId, isAdmin);
            var totalCount = await _todoRepo.CountAsync(countSpec);


            var results = todos.Select(x => new TodoListDto
            {
                Id = x.Id,
                Title = x.Title,
                UserName = x.User?.UserName,
                Description = x.Description,
                CategoryName = x.Category?.Name,
                DueDate = x.ExpiryDate,
                Status = x.Status,
                Priority = x.Priority,
                RecurrenceType = x.RecurrenceType,
                CreatedAt = x.CreatedAt
            }).ToList();

            return new PagedResultDto<TodoListDto>
            {
                Results = results,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task UpdateAsync(int id, TodoUpdateDto todo, int userId, bool isAdmin = false)
        {
            var todoObj = await _todoRepo.GetAll()
                .FirstOrDefaultAsync(x => x.Id == id && (isAdmin || x.UserId == userId));

            if (todoObj == null)
                throw new KeyNotFoundException("Todo not found.");

            if (todo.Status.HasValue && todo.Status.Value != todoObj.Status)
            {
                if (todoObj.ExpiryDate.Date < DateTime.UtcNow.Date && todo.Status.Value == TodoStatus.Pending)
                {
                    throw new InvalidOperationException("Note: You cannot change the status of an expired task. Please edit the 'Expiration Date'.");
                }
            }

            if (!string.IsNullOrWhiteSpace(todo.Title))
                todoObj.Title = todo.Title.Trim();

            if (todo.Description != null)
                todoObj.Description = todo.Description.Trim();

            if (todo.CategoryId.HasValue)
            {
                var categoryExiets = await _categoryRepo.GetById(todo.CategoryId.Value);

                if (categoryExiets == null || (!isAdmin && categoryExiets.UserId != userId))

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
            if (todo.RecurrenceType.HasValue) todoObj.RecurrenceType = todo.RecurrenceType.Value;

            todoObj.UpdatedAt = DateTime.UtcNow;

            _todoRepo.Update(todoObj);
            await _todoRepo.SaveChanges();
        }

    }


}
