using Application.Abstractions;
using Application.DTOs.Requests.ReportRequest;
using Application.DTOs.Responses.ReportResponse;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TalkSpace.Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class ReportsController : BaseApiController
    {
        private readonly IReportService _reportService;

        public ReportsController(
            IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest request)
        {
            var doctorId = User.FindFirstValue("uid");
            var result = await _reportService.GenerateReportAsync(request, doctorId!);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetReportDetails), new { reportId = result.Value!.Id }, result.Value)
                : HandleResult(result);
        }

        [HttpGet("{reportId}/export")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportReportAsPdf(int reportId)
        {
            var result = await _reportService.ExportReportAsPdfAsync(reportId);
            if (!result.IsSuccess)
                return HandleResult(result);

            return File(result.Value, "application/pdf", $"report-{reportId}.pdf");
        }

        [HttpGet("{reportId}")]
        [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportDetails(int reportId)
        {
            var result = await _reportService.GetReportDetailsAsync(reportId);
            return HandleResult(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ReportListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListReports(
            [FromQuery] DateTime? dateRangeStart,
            [FromQuery] DateTime? dateRangeEnd,
            [FromQuery] ReportType? reportType)
        {
            var result = await _reportService.ListReportsAsync(dateRangeStart, dateRangeEnd, reportType);
            return HandleResult(result);
        }

        [HttpDelete("{reportId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            var result = await _reportService.DeleteReportAsync(reportId);
            return result.IsSuccess
                ? NoContent()
                : HandleResult(result);
        }
    }
}