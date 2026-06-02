using Application.Dtos.Category;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications;
using Domain.Entities;


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

        public async Task CreateAsync(CategoryCreateDto categoryDto)
        {
            var userId = _currentUserService.UserId;
            var normalizedName = categoryDto.Name.Trim();

            var spec = new SpecificationBuilder<Category>()
                .Where(c => c.Name == normalizedName && c.UserId == userId)
                .Build();

            var exists = await _categoryRepo.AnyAsync(spec);

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

            var categorySpec = new SpecificationBuilder<Category>()
                .Where(c => c.Id == id && (isAdmin || c.UserId == currentUserId))
                .Build();
            var category = await _categoryRepo.GetEntityWithSpec(categorySpec);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }
            var todosSpec = new SpecificationBuilder<Todo>()
                .Where(t => t.CategoryId == id && (isAdmin || t.UserId == currentUserId))
                .Build();
            var todos = await _todoRepo.ListWithSpecAsync(todosSpec);

            if (deleteLinkedTodos)
            {
                await _todoRepo.DeleteRange(todos.ToList());
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

            await _categoryRepo.Delete(category);
            await _categoryRepo.SaveChanges();
        }

        public async Task<List<CategoryListDto>> GetAllAsync(CategoryFilterDto filter)
        {
            var userId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;

            var builder = new SpecificationBuilder<Category>()
               .Include(t => t.Todos);

            if (!isAdmin)
            {
                builder.Where(c => c.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var normalizedName = filter.Name.Trim().ToLower();
                builder.Where(c => c.Name.Contains(normalizedName));
            }
            var spec = builder.Build();
            var categories = await _categoryRepo.ListWithSpecAsync(spec);

            return categories.Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                TodoCount = c.Todos.Count(t => isAdmin || t.UserId == userId)
            }).ToList();
        }

        public async Task<CategoryListDto?> GetByIdAsync(int id)
        {
            var userId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;

            var builder = new SpecificationBuilder<Category>()
                .Where(c => c.Id == id && (isAdmin || c.UserId == userId))
                .Include(c => c.Todos)
                .Build();
            var category = await _categoryRepo.GetEntityWithSpec(builder);

            if (category == null)
                return null;

            return new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name,
                TodoCount = category.Todos.Count(t => isAdmin || t.UserId == userId)
            };
        }

        public async Task UpdateAsync(int id, CategoryUpdateDto categoryDto)
        {
            var userId = _currentUserService.UserId;

            var spec = new SpecificationBuilder<Category>()
                .Where(c => c.Id == id && c.UserId == userId)
                .Build();

            var categoryinput = await _categoryRepo.GetEntityWithSpec(spec);

            if (categoryinput == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found or you don't have permission to edit it.");
            }
            if (!string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                var normalizedName = categoryDto.Name.Trim();

                var exists = new SpecificationBuilder<Category>()
                    .Where(c => c.Id != id &&
                        c.UserId == userId &&
                        c.Name == normalizedName)
                    .Build();

                var existsCategory = await _categoryRepo.AnyAsync(exists);

                if (existsCategory)
                {
                    throw new InvalidOperationException($"A category with the name '{normalizedName}' already exists.");
                }
                categoryinput.Name = normalizedName;
            }

            _categoryRepo.Update(categoryinput);
            await _categoryRepo.SaveChanges();
        }
    }
}
