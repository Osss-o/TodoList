using Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface IUserService
    {
        Task <UserListDto?> GetByIdAsync(int id);
        Task<List<UserListDto>> GetAllAsync(UserFilterDto fitler);
        Task CreateAsync(UserCreateDto user);
        Task UpdateAsync(UserUpdateDto user,int id);
        Task DeleteAsync(int id);
        Task PromoteToAdminAsync(int id);
    }
}
