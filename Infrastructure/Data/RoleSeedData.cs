using Domain;
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
            if (!context.Users.Any())
            {
                var passwordHasher = new PasswordHasher<User>();
                var admin = new User
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    Role = RoleEnum.Admin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                admin.Password = passwordHasher.HashPassword(admin, "Admin@123");
               
                context.Users.Add(admin);
                await context.SaveChangesAsync();

            }
        }
    }
}
