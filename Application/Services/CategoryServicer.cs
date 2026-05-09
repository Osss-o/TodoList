using Application.Dtos.Category;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly ICurrentUserService _currentUserService;

        public CategoryService(IGenericRepository<Category> categoryRepo, IGenericRepository<Todo> todoRepo, ICurrentUserService currentUserService)
        {
            _categoryRepo = categoryRepo;
            _todoRepo = todoRepo;
            _currentUserService = currentUserService;
        }

        public async Task CreateAsync(CategoryCreateDto categoryDto )
        {
            var userId = _currentUserService.UserId;
            var normalizedName = categoryDto.Name.Trim().ToLower();

            var exists = await _categoryRepo.GetQuery()
                .AnyAsync(c => c.Name.ToLower() == normalizedName && c.UserId==userId);
        
            if (exists)
            {
                throw new InvalidOperationException($"A category with the name '{normalizedName}' already exists.");
            }

            var category = new Category
            {
                Name = categoryDto.Name.Trim(),
                UserId = userId,
            };

            await _categoryRepo.Insert(category);
            await _categoryRepo.SaveChanges();

        }

        public async Task DeleteAsync(int id, bool deleteLinkedTodos = false)
        {
            var currentUserId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;

            var category = await _categoryRepo.GetQuery()
                .FirstOrDefaultAsync(c=>c.Id == id &&(isAdmin||c.UserId == currentUserId));

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            var todos = await _todoRepo.GetQuery()
                .Where(t => t.CategoryId == id && (isAdmin || t.UserId == currentUserId))
                .ToListAsync();
            if (deleteLinkedTodos)
            {
                foreach (var todo in todos)
                {
                   _todoRepo.Delete(todo);
                }

                await _todoRepo.SaveChanges();
            }
            else
            {
                foreach (var todo in todos)
                {
                    todo.CategoryId = null; 
                    _todoRepo.Update(todo);
                }
                await _todoRepo.SaveChanges();
            }

            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChanges();
        }

        public async Task<List<CategoryListDto>> GetQueryAsync(CategoryFilterDto filter)
        {
            var userId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;
            var query = _categoryRepo.GetQuery()
                .AsNoTracking();

            if (!isAdmin)
            {
                query = query.Where(c => c.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var normalizedName = filter.Name.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(normalizedName));
            }

            return await query.Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                TodoCount= _todoRepo.GetQuery()
                .Count(t=>t.CategoryId == c.Id &&(isAdmin || t.UserId == userId))
            }).ToListAsync();
        }

        public async Task<CategoryListDto?> GetByIdAsync(int id)
        {
            var userId = _currentUserService.UserId;

            var category = await _categoryRepo.GetQuery()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (category == null) return null;

            return new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task UpdateAsync(int id, CategoryUpdateDto categoryDto)
        {
            var userId = _currentUserService.UserId;

            var categoryinput = await _categoryRepo.GetQuery()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId==userId);

            if (categoryinput == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found or you don't have permission to edit it.");
            }
            if (!string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                var normalizedName = categoryDto.Name.Trim().ToLower();

                var exists = await _categoryRepo.GetQuery()
                    .AnyAsync(c => c.Id != id &&
                    c.UserId == userId &&
                    c.Name.ToLower() == normalizedName);

                if (exists)
                {
                    throw new InvalidOperationException($"A category with the name '{normalizedName}' already exists.");
                }
                categoryinput.Name = categoryDto.Name.Trim();
            }

            _categoryRepo.Update(categoryinput);
            await _categoryRepo.SaveChanges();
        }
    }
}
