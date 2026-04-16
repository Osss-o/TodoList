using Domain.Entities.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Todo
{
    public class TodoCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public RecurrenceType? RecurrenceType { get; set; }
        public IFormFile? File { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
