using Domain.Entities;
using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Todo
{
    public class TodoListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? DueDate { get; set; }
        public TodoStatus? Status { get; set; }
        public Priority? Priority { get; set; }
        public RecurrenceType? RecurrenceType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
