using Application.Dtos.User;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly ICurrentUserService _currentUserService;

        public UserService(IGenericRepository<User> userRepo,
            IGenericRepository<Todo> todoRepo,
            IGenericRepository<Category> categoryRepo,
            ICurrentUserService currentUserService)
        {
            _userRepo = userRepo;
            _todoRepo = todoRepo;
            _categoryRepo = categoryRepo;
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

            var spec = new SpecificationBuilder<User>()
                  .Where(u => u.Email == user.Email.Trim())
                  .Build();

            var exists = await _userRepo.AnyAsync(spec);

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


            var spec = new SpecificationBuilder<User>()
                .Where(u => u.Id == id)
                .Include(u => u.Todos)
                    .Include(u => u.Categories)
                .Build();

            var user = await _userRepo.GetEntityWithSpec(spec);

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
            var specBuilder = new SpecificationBuilder<User>();

            if (!string.IsNullOrEmpty(fitler.UserName))
                specBuilder.Where(u => u.UserName.Contains(fitler.UserName.Trim()));

            if (!string.IsNullOrEmpty(fitler.Email))
                specBuilder.Where(u => u.Email.Contains(fitler.Email.Trim()));

            specBuilder.OrderBy(u => u.CreatedAt, isDescending: true)
                .ApplyPaging((fitler.PageNumber - 1) * fitler.PageSize, fitler.PageSize);

            var spec = specBuilder.Build();
            var users = await _userRepo.ListWithSpecAsync(spec);

            return users.Select(u => new UserListDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                Role = u.Role.ToString()
            }).ToList();
        }


        public async Task<UserListDto?> GetByIdAsync(int id)
        {
            var spec = new SpecificationBuilder<User>()
                .Where(u => u.Id == id)
                .Build();
            var user = await _userRepo.GetEntityWithSpec(spec);

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

                var spec = new SpecificationBuilder<User>()
                    .Where(u => u.Email == normalizedEmail && u.Id != user.Id)
                    .Build();
                var exists = await _userRepo.AnyAsync(spec);

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

            var todoSpcec = new SpecificationBuilder<Todo>()
                .Where(t => t.UserId == id)
                .Build();
            var hasTasks = await _todoRepo.AnyAsync(todoSpcec);

            if (hasTasks)
                throw new Exception("Connot promote user : This account has active tasks.Admin accounts must be clean.");

            var caregorySpec = new SpecificationBuilder<Category>()
                .Where(c => c.UserId == id)
                .Build();
            var hasCategories = await _categoryRepo.AnyAsync(caregorySpec);

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
