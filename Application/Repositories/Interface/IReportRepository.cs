using Application.Dtos.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Interface
{
    public interface IReportRepository
    {
        Task<List<UserProductivityResponseDto>> GetUserProductivityResponsesAsync(UserProductivityFilterDto filter);
        Task<List<CategoryUsageResponseDto>>GetCategoryUsagesAsync(CategoryUsageFilterDto filter);
    }
}
