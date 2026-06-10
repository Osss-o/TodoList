using Application.Dtos.Reports;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Interface
{
    public interface IReportQueryService
    {
        Task<List<UserTodosData>> GetUserProductivityDataAsync(ISpecification<User>spec,UserProductivityFilterDto filter);
        Task<List<CategoryTodosData>>GetCategoryUsagesDataAsync(ISpecification<Category>spec,CategoryUsageFilterDto filter);
    }
}
