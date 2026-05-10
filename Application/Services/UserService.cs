using Application.Dtos.User;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly IGenericRepository<Category> _gategoryRepo;
        private readonly ICurrentUserService _currentUserService;

        public UserService(IGenericRepository<User> userRepo,
            IGenericRepository<Todo> todoRepo,
            IGenericRepository<Category> gategoryRepo,
            ICurrentUserService currentUserService)
        {
            _userRepo = userRepo;
            _todoRepo = todoRepo;
            _gategoryRepo = gategoryRepo;
            _currentUserService = currentUserService;
        }

        public async Task CreateAsync(UserCreateDto user)
        {
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$";
            if (!Regex.IsMatch(user.Password, passwordPattern))
                throw new Exception("Password is weak.");

            string emailPattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}$";
            if (!Regex.IsMatch(user.Email, emailPattern))
                throw new Exception("Email is not valid.");

            var exists = await _userRepo.GetQuery()
                .AnyAsync(u => u.Email == user.Email);

            if (exists)
                throw new Exception("Email is already in use.");

            var newUser = new User
            {
                UserName = user.UserName.Trim(),
                Email = user.Email.Trim(),
                Password = new PasswordHasher<User>().HashPassword(null, user.Password),
                CreatedAt = DateTime.UtcNow,
                Role = RoleEnum.User,
            };
            await _userRepo.Insert(newUser);
            await _userRepo.SaveChanges();
        }

        public async Task DeleteAsync(int id)
        {
            var isAdmin = _currentUserService.IsAdmin;
            var currentUserId = _currentUserService.UserId;


            var user = await _userRepo.GetQuery()
                .Include(u => u.Todos)
                .Include(u => u.Categories)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new Exception("User not found.");

            if (user.Email == SuperAdmin.Email)
                throw new Exception("Default admin cannot be deleted.");

            if (!isAdmin && id != currentUserId)
                throw new UnauthorizedAccessException("You don't have permission to delete this user.");

            _userRepo.Delete(user);
            await _userRepo.SaveChanges();
        }

        public async Task<List<UserListDto>> GetAllAsync(UserFilterDto fitler)
        {
            var query = _userRepo.GetQuery().AsNoTracking();

            if (!string.IsNullOrEmpty(fitler.UserName))
                query = query.Where(u => u.UserName.Contains(fitler.UserName.Trim()));

            if (!string.IsNullOrEmpty(fitler.Email))
                query = query.Where(u => u.Email.Contains(fitler.Email.Trim()));

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((fitler.PageNumber - 1) * fitler.PageSize)
                .Take(fitler.PageSize)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                    Role = u.Role.ToString()
                }).ToListAsync();

            return users;
        }

        public async Task<UserListDto?> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;
            return new UserListDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task UpdateAsync(UserUpdateDto userDto)
        {
            var isAdmin = _currentUserService.IsAdmin;
            var currentUserId = _currentUserService.UserId;


            if (!isAdmin && currentUserId != currentUserId)
                throw new UnauthorizedAccessException("You are not allowed to update this user.");

            var user = await _userRepo.GetById(currentUserId);

            if (user == null)
                throw new Exception("User not found.");

            if (user.Email == SuperAdmin.Email && !string.IsNullOrEmpty(userDto.Email))
            {
                if (userDto.Email.Trim().ToLower() != SuperAdmin.Email.ToLower())
                    throw new Exception("Default admin cannot be updated.");
            }
            if (!string.IsNullOrEmpty(userDto.UserName))

                user.UserName = userDto.UserName;

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                string emailPattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}$";
                if (!Regex.IsMatch(userDto.Email, emailPattern))
                    throw new Exception("Email is not valid.");

                var normalizedEmail = userDto.Email.Trim().ToLower();

                var exists = await _userRepo.GetQuery()
                    .AnyAsync(u => u.Email == normalizedEmail && u.Id != user.Id);

                if (exists)
                    throw new Exception("Email is already in use.");

                user.Email = normalizedEmail;
            }
            user.UpdatedAt = DateTime.UtcNow;
            _userRepo.Update(user);

            await _userRepo.SaveChanges();
        }

        public async Task PromoteToAdminAsync(int id)
        {
            var user = await _userRepo.GetById(id);

            if (user == null)
                throw new Exception("User not found.");

            if (user.Role == RoleEnum.Admin)
                throw new Exception("User is already an admin.");

            var hasTasks = await _todoRepo.GetQuery()
               .AnyAsync(t => t.UserId == id);

            if (hasTasks)
                throw new Exception("Connot promote user : This account has active tasks.Admin accounts must be clean.");

            var hasCategories = await _gategoryRepo.GetQuery()
                .AnyAsync(c => c.UserId == id);

            if (hasCategories)
                throw new Exception("Cannat promote user:This accounr has existing category.");

            user.Role = RoleEnum.Admin;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }

        public async Task DemoteFromAdminAsync(int id, RoleEnum role)
        {
            if (role != RoleEnum.SuperAdmin)
                throw new UnauthorizedAccessException("Only super admins can demote admins.");

            var user = await _userRepo.GetById(id);

            if (user == null)
                throw new Exception("User not found.");

            if (user.Email == SuperAdmin.Email)
                throw new Exception("Default admin cannot be demoted.");

            if (user.Role != RoleEnum.Admin)
                throw new Exception("User is not an admin.");

            user.Role = RoleEnum.User;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }
    }
}
