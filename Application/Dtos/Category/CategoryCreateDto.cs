using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Category
{
    public class CategoryCreateDto
    {
        
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
      //  public double Progress { get; set; } = 0;
    }
}
