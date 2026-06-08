using Application.Dtos.Reports;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications;
using Domain.Entities;
using Domain.Entities.Enums;

namespace Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepo;
        public ReportService(IReportRepository reportRepo)

        {
            _reportRepo = reportRepo;
        }

        public async Task<List<UserProductivityResponseDto>> GetUserProductivityReportAsync(UserProductivityFilterDto filter)
        {
            ValidateDates(filter.FromDate, filter.ToDate);

            return await _reportRepo.GetUserProductivityResponsesAsync(filter);
        }
        public async Task<List<CategoryUsageResponseDto>> GetCategoryUsageReportAsync(CategoryUsageFilterDto filter)
        {
            ValidateDates(filter.FromDate, filter.ToDate);

            return await _reportRepo.GetCategoryUsagesAsync(filter);
        }
        private void ValidateDates(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue)
            {
                if (fromDate.Value > toDate.Value)
                {
                    throw new ArgumentException("FromDate cannot be later than ToDate.");
                }
            }
            if (fromDate.HasValue && fromDate.Value > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("FromDate cannot be in the future.");
            }
            if (toDate.HasValue && toDate.Value > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("ToDate cannot be in the future.");
            }
        }
    }
}
