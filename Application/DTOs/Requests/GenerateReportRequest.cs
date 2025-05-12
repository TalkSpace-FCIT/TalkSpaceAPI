
using Domain.Enums;

namespace Application.DTOs.Requests
{
    public record GenerateReportRequest(
        DateTime DateRangeStart, 
        DateTime DateRangeEnd, 
        ReportType ReportType);
}
