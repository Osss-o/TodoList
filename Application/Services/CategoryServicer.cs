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

        public CategoryService(IGenericRepository<Category> categoryRepo, IGenericRepository<Todo> todoRepo)
        {
            _categoryRepo = categoryRepo;
            _todoRepo = todoRepo;
        }

        public async Task CreateAsync(CategoryCreateDto categoryDto, int userId)
        {
            var normalizedName = categoryDto.Name.Trim().ToLower();

            var exists = await _categoryRepo.GetAll()
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

        public async Task DeleteAsync(int id, int currentUserId, bool isAdmin, bool deleteLinkedTodos = false)
        {
            var category = await _categoryRepo.GetAll()
                .FirstOrDefaultAsync(c=>c.Id == id &&(isAdmin||c.UserId == currentUserId));


            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            var todos = await _todoRepo.GetAll()
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

        public async Task<List<CategoryListDto>> GetAllAsync(CategoryFilterDto filter, int userId, bool isAdmin = false)
        {
            var query = _categoryRepo.GetAll()
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
                Name = c.Name
            }).ToListAsync();
        }

        public async Task<CategoryListDto?> GetByIdAsync(int id, int userId)
        {
            var category = await _categoryRepo.GetAll()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (category == null) return null;

            return new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task UpdateAsync(int id,int userId, CategoryUpdateDto categoryDto)
        {
            var categoryinput = await _categoryRepo.GetAll()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId==userId);

            if (categoryinput == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found or you don't have permission to edit it.");
            }
            if (!string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                var normalizedName = categoryDto.Name.Trim().ToLower();

                var exists = await _categoryRepo.GetAll()
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
