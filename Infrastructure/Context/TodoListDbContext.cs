using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Reflection.Emit;

namespace Infrastructure.Context
{
    public class TodoListDbContext : DbContext
    {
        public TodoListDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Todo> Todo { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<FileAttachment> Attachments { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var relationShips = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys());

            foreach (var relationShip in relationShips)
            {
                if( relationShip.PrincipalEntityType.ClrType == typeof(User))
                {
                    relationShip.DeleteBehavior = DeleteBehavior.Cascade;
                }
                else
                {
                    relationShip.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }

            modelBuilder.Entity<Todo>()
               .HasOne(t => t.Category)
               .WithMany(c => c.Todos)
               .HasForeignKey(t => t.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FileAttachment>()
                .HasOne(a => a.Todo)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TodoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
