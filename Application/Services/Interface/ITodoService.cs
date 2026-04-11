using Application.Dtos.PagedResult;
using Application.Dtos.Todo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface ITodoService
    {
        Task<TodoListDto?> GetByIdAsync(int id,int userId);
        Task<List<TodoListDto>> GetAllAsync(TodoFilterDto filter,int userId);
        Task CreateAsync(TodoCreateDto todo, int userId);
        Task UpdateAsync(int id, TodoUpdateDto todo, int userId);
        Task DeleteAsync(int id, int userId);
        Task<PagedResultDto<TodoListDto>> SearchAsync(TodoFilterDto filter, int userId, bool isAdmin = false);
    }
}
