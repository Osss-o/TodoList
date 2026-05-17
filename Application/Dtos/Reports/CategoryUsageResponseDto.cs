using Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Reports
{
    public class CategoryUsageResponseDto
    {
        public string CategoryName { get; set; }
        public string CategoryOwner { get; set; }
        public int TotalLinkedTodos { get; set; }
        public int CompletedTodos { get; set; }
        public int PendingTodos { get; set; }
        public int ExpiredTodos { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public Priority? MostCommonPriority { get; set; }
        public int RecurringTodosCount { get; set; }
        public bool SafeToDelete { get; set; }
    }
       
}
