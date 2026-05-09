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
        Task<TodoListDto?> GetByIdAsync(int id);
        Task<List<TodoListDto>> GetQueryAsync(TodoFilterDto filter);
        Task CreateAsync(TodoCreateDto todo);
        Task UpdateAsync(int id, TodoUpdateDto todo);
        Task DeleteAsync(int id);
        Task<PagedResultDto<TodoListDto>> SearchAsync(TodoFilterDto filter);
    }
}
