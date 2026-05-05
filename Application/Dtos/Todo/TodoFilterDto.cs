using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Todo
{
    public class TodoFilterDto
    {
        public string? Title { get; set; }
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
        public TodoStatus? Status { get; set; }
        public Priority? Priority { get; set; }
        public RecurrenceType? RecurrenceType { get; set; }
        public DateTime? FromeDate { get; set; }
        public DateTime? ToDate { get; set; }
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value >50?50:value);
        }
       

    }
}
