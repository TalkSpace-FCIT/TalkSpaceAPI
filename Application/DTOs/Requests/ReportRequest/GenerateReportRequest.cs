using Domain.Enums;

namespace Application.DTOs.Requests.ReportRequest
{
    public record GenerateReportRequest(
        DateTime DateRangeStart, 
        DateTime DateRangeEnd, 
        ReportType ReportType);
}
