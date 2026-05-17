using Application.Dtos.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface IReportService
    {
        Task<List<UserProductivityResponseDto>> GetUserProductivityReportAsync(UserProductivityFilterDto filter);
        Task<List<CategoryUsageResponseDto>> GetCategoryUsageReportAsync(CategoryUsageFilterDto filter);
    }
}
