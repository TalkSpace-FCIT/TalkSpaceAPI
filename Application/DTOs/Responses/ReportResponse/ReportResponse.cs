using Domain.Enums;

namespace Application.DTOs.Responses.ReportResponse
{
    public record ReportResponse(
        int Id, 
        DateTime DateRangeStart,
        DateTime DateRangeEnd,
        ReportType ReportType,
        DateTime GeneratedDate,
        string DoctorName,
        string DownloadUrl);

    public record ReportListResponse(
        IEnumerable<ReportResponse> Reports,
        int TotalCount);
}
