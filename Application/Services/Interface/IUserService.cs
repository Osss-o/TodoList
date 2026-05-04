using Application.Dtos.User;
using Domain.Entities.Enums;
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
        Task UpdateAsync(UserUpdateDto user,int id,int currentUserId, bool isAdmin);
        Task DeleteAsync(int id, int currentUserId, bool isAdmin);
        Task PromoteToAdminAsync(int id);
        Task DemoteFromAdminAsync(int id,RoleEnum role);
    }
}
