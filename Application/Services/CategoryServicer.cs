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

        public async Task Delete(int id)
        {
            var category = await _categoryRepo.GetById(id);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }
            var todos = await _todoRepo.GetAll()
                .Where(t => t.CategoryId == id)
                .ToListAsync();

            foreach (var todo in todos)
            {
                todo.CategoryId = null;
                _todoRepo.Update(todo);
            }

            await _todoRepo.SaveChanges();

            _categoryRepo.Delete(category);
            await _categoryRepo.SaveChanges();
        }

        public async Task<List<CategoryListDto>> GetAllAsync(CategoryFilterDto filter)
        {
            var query = _categoryRepo.GetAll();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var normalizedName = filter.Name.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(normalizedName));
            }

            var categories = await query.ToListAsync();

            return categories.Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }

        public async Task<CategoryListDto?> GetByIdAsync(int id)
        {
            var category = await _categoryRepo.GetById(id);
            if (category == null) return null;

            return new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task UpdateAsync(int id, CategoryUpdateDto categoryDto)
        {
            var categoryinput = await _categoryRepo.GetById(id);

            if (categoryinput == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }
            if (!string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                var normalizedName = categoryDto.Name.Trim().ToLower();

                var exists = await _categoryRepo.GetAll()
                    .AnyAsync(c => c.Id != id && c.Name.ToLower() == normalizedName);
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
