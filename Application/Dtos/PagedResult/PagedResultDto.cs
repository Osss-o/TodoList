using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.PagedResult
{
    public class PagedResultDto<T>
    {
        public List<T> Results { get; set; } = new();
        public int TotalCount { get; set; }          
        public int PageNumber { get; set; }         
        public int PageSize { get; set; }         
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    }
}
