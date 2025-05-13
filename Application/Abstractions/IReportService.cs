using Application.DTOs.Requests.ReportRequest;
using Application.DTOs.Responses.ReportResponse;
using Domain.Enums;
using Domain.Results;

namespace Application.Abstractions
{
    public interface IReportService
    {
        Task<Result<ReportResponse>> GenerateReportAsync(GenerateReportRequest request, string doctorId);
        Task<Result<byte[]>> ExportReportAsPdfAsync(int reportId);
        Task<Result<ReportResponse>> GetReportDetailsAsync(int reportId);
        Task<Result<ReportListResponse>> ListReportsAsync(DateTime? dateRangeStart, DateTime? dateRangeEnd, ReportType? reportType);
        Task<Result<bool>> DeleteReportAsync(int reportId);
    }
}
