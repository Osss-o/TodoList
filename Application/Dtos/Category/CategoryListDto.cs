using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Category
{
    public class CategoryListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TodoCount { get; set; }
        public int? ParentCategoryId { get; set; }
        public double Progress { get; set; }
        public TodoStatus Status { get; set; }

    }
}
