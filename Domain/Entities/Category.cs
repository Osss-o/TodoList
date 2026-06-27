using Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public int? ParentCategoryId { get; set; }
        [ForeignKey("ParentCategoryId")]
        public Category ParentCategory { get; set; }
        public List<Category> SubCategories { get; set; } = new List<Category>();
        public double Progress { get; set; } = 0;
        public TodoStatus Status { get; set; } = TodoStatus.Pending;
        public List<Todo> Todos { get; set; }= new List<Todo>();    
    }
}
