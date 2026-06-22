using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Dtos.Reports
{
    public class UserProductivityResponseDto
    {
        public string UserName { get; set; }
        public int TotalTodos { get; set; }
        public int CompletedTodos { get; set; }
        public int PendingTodos { get; set; }
        public double CompletionRate { get; set; }
        public int HighPriorityTodos { get; set; }
        public int ExpiredTodos { get; set; }
        public double AverageCompletionTime { get; set; }

    }
}
