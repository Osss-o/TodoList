using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Enums;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class RoleSeedData
    {
        public static async Task InitializeAsync(TodoListDbContext context)
        {
            if (!context.Users.Any(u=>u.Email == DefaultAdmin.Email))
            {
                var passwordHasher = new PasswordHasher<User>();

                var admin = new User
                {
                    UserName = DefaultAdmin.UserName,
                    Email = DefaultAdmin.Email,
                    Role = RoleEnum.Admin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                admin.Password = passwordHasher.HashPassword(admin, DefaultAdmin.Password);
               
                context.Users.Add(admin);
                await context.SaveChangesAsync();

            }
        }
    }
}
