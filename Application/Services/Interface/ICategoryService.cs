using Application.Dtos.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface ICategoryService
    {
        Task<CategoryListDto?> GetByIdAsync(int id, int userId);
        Task <List<CategoryListDto>> GetAllAsync(CategoryFilterDto filter, int userId);
        Task CreateAsync(CategoryCreateDto categoryDto, int userId);
        Task UpdateAsync(int id,int userId, CategoryUpdateDto category);
        Task DeleteAsync(int id, int currentUserId, bool isAdmin, bool deleteLinkedTodos = false);
    }
}
