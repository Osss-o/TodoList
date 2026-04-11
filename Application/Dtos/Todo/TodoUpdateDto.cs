using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Todo
{
    public class TodoUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority? Priority { get; set; }
        public TodoStatus? Status { get; set; }
        public RecurrenceType? RecurrenceType { get; set; }
    }
}
