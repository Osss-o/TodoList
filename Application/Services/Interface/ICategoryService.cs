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
        Task<CategoryListDto?> GetByIdAsync(int id);
        Task <List<CategoryListDto>> GetAllAsync(CategoryFilterDto filter);
        Task CreateAsync(CategoryCreateDto categoryDto, int userId);
        Task UpdateAsync(int id, CategoryUpdateDto category);
        Task Delete(int id);
    }
}
