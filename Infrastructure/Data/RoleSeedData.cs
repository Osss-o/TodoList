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
            var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == SuperAdmin.Email);


            if (existingAdmin == null)
            {
                var passwordHasher = new PasswordHasher<User>();

                var admin = new User
                {
                    UserName = SuperAdmin.UserName,
                    Email = SuperAdmin.Email,
                    Role = RoleEnum.SuperAdmin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                admin.Password = passwordHasher.HashPassword(admin, SuperAdmin.Password);

                context.Users.Add(admin);
                await context.SaveChangesAsync();

            }
            else if (existingAdmin.Role != RoleEnum.SuperAdmin)
            {
                {
                    existingAdmin.Role = RoleEnum.SuperAdmin;
                    context.Users.Update(existingAdmin);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
