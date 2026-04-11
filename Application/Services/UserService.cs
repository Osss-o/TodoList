using Application.Dtos.User;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepo;

        public UserService(IGenericRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task CreateAsync(UserCreateDto user)
        {
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$";
            if (!Regex.IsMatch(user.Password, passwordPattern))
                throw new Exception("Password is weak.");

            string emailPattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}$";
            if (!Regex.IsMatch(user.Email, emailPattern))
                throw new Exception("Email is not valid.");

            var exists = await _userRepo.GetAll()
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
            var user = await _userRepo.GetById(id);
            if (user == null)
                throw new Exception("User not found.");
            _userRepo.Delete(user);
            await _userRepo.SaveChanges();
        }

        public async Task<List<UserListDto>> GetAllAsync(UserFilterDto fitler)
        {
            var query = _userRepo.GetAll().AsNoTracking();

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
                    CreatedAt = u.CreatedAt
                }).ToListAsync();

            return users;
        }

        public async Task<UserListDto?> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetAll()
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

        public async Task UpdateAsync(UserUpdateDto userDto, int id)
        {
            var user = await _userRepo.GetById(id);

            if (user == null)
                throw new Exception("User not found.");

            if (!string.IsNullOrEmpty(userDto.UserName))

                user.UserName = userDto.UserName;

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                string emailPattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}$";
                if (!Regex.IsMatch(userDto.Email, emailPattern))
                    throw new Exception("Email is not valid.");

                user.Email = userDto.Email.Trim();

                var exists = await _userRepo.GetAll()
                    .AnyAsync(u => u.Email == user.Email && u.Id != id);
                if (exists)
                    throw new Exception("Email is already in use.");
                user.Email = userDto.Email.Trim();
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

            user.Role = RoleEnum.Admin;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }
    }
}
