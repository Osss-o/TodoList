using Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(250)]
        public string Email { get; set; }
        [Required]
        [MinLength(8)]
        public string Password{ get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }
        [Required]
        
        public RoleEnum Role { get; set; }
        public ICollection<Todo> Todos { get; set; }
        public ICollection<Category> Categories { get; set; }
 
    }
}
