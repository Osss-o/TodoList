using Application.Dtos.Category;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications;
using Domain.Entities;
using Domain.Entities.Enums;


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
            if (categoryDto.ParentCategoryId.HasValue)
            {
                var parentSpec = new SpecificationBuilder<Category>()
                    .Where(c => c.Id == categoryDto.ParentCategoryId.Value && c.UserId == userId)
                    .Build();
                if (!await _categoryRepo.AnyAsync(parentSpec))
                {
                    throw new KeyNotFoundException($"Parent category with ID {categoryDto.ParentCategoryId.Value} not found.");
                }
            }

            var category = new Category
            {
                Name = categoryDto.Name.Trim(),
                UserId = userId,
                ParentCategoryId = categoryDto.ParentCategoryId,
                Progress =0,
                Status = TodoStatus.Pending
            };

            await _categoryRepo.Insert(category);
            await _categoryRepo.SaveChanges();

            await UpdateParentProgressAsync(category.ParentCategoryId);

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
            }
            else
            {
                foreach (var todo in todos)
                {
                    todo.CategoryId = null;
                    _todoRepo.Update(todo);
                }
            }
            var subCategoriesSpec = new SpecificationBuilder<Category>()
                .Where(c=>c.ParentCategoryId==id)
                .Build();
            var subCategories = await _categoryRepo.ListWithSpecAsync(subCategoriesSpec);

            foreach (var sub in subCategories)
            {
                sub.ParentCategoryId = null;
                _categoryRepo.Update(sub);
            }

            int? parentId =category.ParentCategoryId;

            await _categoryRepo.Delete(category);
            await _categoryRepo.SaveChanges();

            await UpdateParentProgressAsync(parentId);
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
                TodoCount = c.Todos.Count(t => isAdmin || t.UserId == userId),
                ParentCategoryId = c.ParentCategoryId,
                Progress = c.Progress,
                Status = c.Status
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
                TodoCount = category.Todos.Count(t => isAdmin || t.UserId == userId),
                ParentCategoryId = category.ParentCategoryId,
                Progress = category.Progress,
                Status = category.Status
            };
        }

        public async Task UpdateAsync(int id, CategoryUpdateDto categoryDto)
        {
            var userId = _currentUserService.UserId;

            var spec = new SpecificationBuilder<Category>()
                .Where(c => c.Id == id && c.UserId == userId)
                .Include(c => c.Todos)
                .Build();

            var categoryInput = await _categoryRepo.GetEntityWithSpec(spec);

            if (categoryInput == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found or you don't have permission to edit it.");
            }
            if (!string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                var normalizedName = categoryDto.Name.Trim();

                var exists = new SpecificationBuilder<Category>()
                    .Where(c => c.Id != id && c.UserId == userId && c.Name == normalizedName)
                    .Build();

                if (await _categoryRepo.AnyAsync(exists))
                {
                    throw new InvalidOperationException($"A category with the name '{normalizedName}' already exists.");
                }
                categoryInput.Name = normalizedName;
            }
            int? oldParentId = categoryInput.ParentCategoryId;

            if (categoryDto.ParentCategoryId != categoryInput.ParentCategoryId)
            {
                if (categoryDto.ParentCategoryId == id)
                    throw new InvalidOperationException("A category cannot be its own parent.");
                categoryInput.ParentCategoryId = categoryDto.ParentCategoryId;
            }
            _categoryRepo.Update(categoryInput);
            await _categoryRepo.SaveChanges();
            if (oldParentId != categoryInput.ParentCategoryId)
            {
                await UpdateParentProgressAsync(oldParentId);
                await UpdateParentProgressAsync(categoryInput.ParentCategoryId);
            }
        }
        private async Task UpdateParentProgressAsync(int? parentCategoryId)
        {
            if (!parentCategoryId.HasValue) return;

                var parentSpec = new SpecificationBuilder<Category>()
                         .Where(c => c.Id == parentCategoryId.Value)
                         .Include(c => c.SubCategories)
                         .Build();

            var parent = await _categoryRepo.GetEntityWithSpec(parentSpec);
            if (parent == null) return;

            if (parent.SubCategories.Any())
            {
                parent.Progress =Math.Round(parent.SubCategories.Average(c => c.Progress), 2);
            }
            else
            {
                parent.Progress = 0;
            }
            parent.Status = parent.Progress >= 100 ? TodoStatus.Done : TodoStatus.Pending;

            _categoryRepo.Update(parent);
            await _categoryRepo.SaveChanges();

            if (parent.ParentCategoryId.HasValue)
            {
                await UpdateParentProgressAsync(parent.ParentCategoryId);
            }
        }

    }
}
