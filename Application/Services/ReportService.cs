
using Application.Abstractions;
using Application.DTOs.Requests.ReportRequest;
using Application.DTOs.Responses.ReportResponse;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<Report> _reportRepository;
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IStorageService _storageService;
        public ReportService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPdfGenerator pdfGenerator,
            IStorageService storageService,
            ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _pdfGenerator = pdfGenerator;
            _storageService = storageService;
            _reportRepository = unitOfWork.GetRepository<Report>();
            _doctorRepository = unitOfWork.GetRepository<Doctor>();
        }
        public async Task<Result<bool>> DeleteReportAsync(int reportId)
        {
            try
            {
                Log.Information("Deleting report {ReportId}", reportId);

                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                {
                    Log.Warning("Report not found: {ReportId}", reportId);
                    return Result<bool>.Failure("Report not found", ErrorSource.TalkSpaceAPI);
                }

                _reportRepository.Remove(report);
                await _unitOfWork.CommitAsync();

                Log.Information("Successfully deleted report {ReportId}", reportId);
                return Result<bool>.Success(true, "report deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting report {ReportId}", reportId);
                return Result<bool>.Failure("Error deleting report", ErrorSource.Database);
            }
        }

        public async Task<Result<byte[]>> ExportReportAsPdfAsync(int reportId)
        {
            try
            {
                Log.Information("Exporting report {ReportId} as PDF", reportId);

                // First try to retrieve from storage 

                try
                {
                    var storedPdf = await _storageService.RetrieveReportAsync(reportId);
                    return Result<byte[]>.Success(storedPdf, "Report exported successfully");
                }
                catch (FileNotFoundException)
                {
                    // If not found in storage, generate new PDF. 
                    var report = await _reportRepository.GetByIdAsync(reportId);
                    if(report == null)
                    {
                        Log.Warning("Report not found: {ReportId}", reportId);
                        return Result<byte[]>.Failure("Report not found", ErrorSource.TalkSpaceAPI);
                    }

                    var pdfBytes = await _pdfGenerator.GeneratePdfAsync(report.Content);

                    // Store the generated PDF for future use
                    await _storageService.StoreReportAsync(reportId, pdfBytes);

                    return Result<byte[]>.Success(pdfBytes, "Report exported successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exporting report {ReportId} as PDF", reportId);
                return Result<byte[]>.Failure("Error exporting report", ErrorSource.Database);
            }
        }

        public async Task<Result<ReportResponse>> GenerateReportAsync(GenerateReportRequest request, string doctorId)
        {
            try
            {
                Log.Information("Generating {ReportType} report for doctor {DoctorId}", request.ReportType, doctorId);

                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    Log.Warning("Doctor not found: {DoctorId}", doctorId);
                    return Result<ReportResponse>.Failure("Doctor not found", ErrorSource.TalkSpaceAPI);
                }

                // Generate report content (implementation specific). 
                var reportContent = await GenerateReportContent(request, doctorId);
                var pdfBytes = await _pdfGenerator.GeneratePdfAsync(reportContent);

                var report = new Report
                {
                    DateRangeStart = request.DateRangeStart,
                    DateRangeEnd = request.DateRangeEnd,
                    Type = request.ReportType,
                    GeneratedDate = DateTime.UtcNow,
                    Content = reportContent,
                    DoctorId = doctorId
                };

                await _reportRepository.AddAsync(report);
                await _unitOfWork.CommitAsync();

                // Store the PDF and get the storage path
                var storagePath = await _storageService.StoreReportAsync(report.Id, pdfBytes);

                // Generate download URL (implementation specific). 
                var downloadUrl = $"/api/reports/{report.Id}/export";

                Log.Information("Successfully generated report ID: {ReportID}", report.Id);

                return Result<ReportResponse>.Success(new ReportResponse(
                    report.Id,
                    report.DateRangeStart,
                    report.DateRangeEnd,
                    report.Type,
                    report.GeneratedDate,
                    doctor.FullName,
                    downloadUrl), "Report generated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating report");
                return Result<ReportResponse>.Failure("Error generating report", ErrorSource.Database);
            }
        }

        public async Task<Result<ReportResponse>> GetReportDetailsAsync(int reportId)
        {
            try
            {
                Log.Information("Fetching details for report {ReportId}", reportId);

                var report = await _reportRepository.GetByIdAsync(reportId,
                    include: q => q.Include(r => r.Doctor));

                if(report == null)
                {
                    Log.Warning("Report not found: {ReportId}", reportId);
                    return Result<ReportResponse>.Failure("Report not found", ErrorSource.TalkSpaceAPI);
                }

                var downloadUrl = $"/api/reports/{report.Id}/export";
                var response = new ReportResponse(
                    report.Id,
                    report.DateRangeStart,
                    report.DateRangeEnd,
                    report.Type,
                    report.GeneratedDate,
                    report.Doctor.FullName,
                    downloadUrl);

                return Result<ReportResponse>.Success(response, "report retreived successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching report {ReportId}", reportId);
                return Result<ReportResponse>.Failure("Error fetching report", ErrorSource.Database);
            }
        }

        public async Task<Result<ReportListResponse>> ListReportsAsync(
            DateTime? dateRangeStart,
            DateTime? dateRangeEnd,
            ReportType? reportType)
        {
            try
            {
                Log.Information("Listing reports with filters: {Filters}", new
                {
                    DateRangeStart = dateRangeStart,
                    DateRangeEnd = dateRangeEnd,
                    ReportType = reportType
                });

                var query = _reportRepository.Queryable()
                    .Include(r => r.Doctor)
                    .AsQueryable();

                if (dateRangeStart.HasValue)
                    query = query.Where(r => r.GeneratedDate >= dateRangeStart.Value);

                if (dateRangeEnd.HasValue)
                    query = query.Where(r => r.GeneratedDate <= dateRangeEnd.Value);

                if (reportType.HasValue)
                    query = query.Where(r => r.Type == reportType.Value);

                var reports = await query.ToListAsync();
                var totalCount = await query.CountAsync();

                var response = new ReportListResponse(
                    reports.Select(r => new ReportResponse(
                            r.Id,
                            r.DateRangeStart,
                            r.DateRangeEnd,
                            r.Type,
                            r.GeneratedDate,
                            r.Doctor.FullName,
                            $"/api/reports/{r.Id}/export")),
                    totalCount
                    );

                Log.Debug("Found {ReportCount} reports", totalCount);
                return Result<ReportListResponse>.Success(response, "all reports retrived.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error listing reports");
                return Result<ReportListResponse>.Failure("Error listing reports", ErrorSource.Database);
            }
        }


        #region Report Generation Implementation
        private async Task<string> GenerateReportContent(GenerateReportRequest request, string doctorId)
        {
            try
            {
                Log.Information("Generating content for {ReportType} report", request.ReportType);

                var reportContent = new StringBuilder();
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);

                // Common header for all reports 

                reportContent.AppendLine($"Report Type: {request.ReportType}");
                reportContent.AppendLine($"Date Range: {request.DateRangeStart:d} to {request.DateRangeEnd:d}");
                reportContent.AppendLine($"Generted On: {DateTime.Now:d}");
                reportContent.AppendLine($"Generted By: Dr. {doctor?.FullName ?? "Unknown"}");
                reportContent.AppendLine("----------------------------------------");
                reportContent.AppendLine();

                switch (request.ReportType)
                {
                    case ReportType.Financial:
                        await GenerateFinancialReport(request, reportContent);
                        break;

                    case ReportType.Medical:
                        await GenerateMedicalReport(request, reportContent);
                        break;

                    case ReportType.BillingHistory:
                        await GenerateBillingHistory(request, reportContent);
                        break;

                    case ReportType.AppointmentSummary:
                        await GenerateAppointmentSummary(request, reportContent);
                        break;

                    default:
                        reportContent.AppendLine("No specific content generated for this report type.");
                        break;
                }

                return reportContent.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating report content");
                return $"Error generating report content: {ex.Message}";
            }

        }

        private async Task GenerateFinancialReport(GenerateReportRequest request, StringBuilder reportContent)
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .FindAsync(a => a.DateTime >= request.DateRangeStart &&
                a.DateTime <= request.DateRangeEnd);

            var payments = await _unitOfWork.GetRepository<Payment>()
                .FindAsync(p => p.PaymentDate >= request.DateRangeStart &&
                p.PaymentDate <= request.DateRangeEnd);

            // Generate financial summary 

            reportContent.AppendLine("FINANCIAL SUMMARY");
            reportContent.AppendLine("=================");
            reportContent.AppendLine($"Total Appointments: {appointments.Count()}");
            reportContent.AppendLine($"Completed Appointments: {appointments.Count(a => a.Status == AppointmentStatus.Completed)}");
            reportContent.AppendLine($"Total Revenue: {payments.Sum(p => p.Amount):C}");
            reportContent.AppendLine($"Outstanding Payments: {payments.Where(p => !p.IsPaid).Sum(p => p.Amount):C}");
            reportContent.AppendLine();

            // Detailed appointment breakdown
            reportContent.AppendLine("APPOINTMENT BREAKDOWN");
            reportContent.AppendLine("---------------------");
            
            foreach ( var appointment in appointments.OrderBy(a => a.DateTime))
            {
                reportContent.AppendLine($"{appointment.DateTime:d} - {appointment.Patient.FullName} - {appointment.Status}");
            }

        }

        private async Task GenerateMedicalReport(GenerateReportRequest request, StringBuilder content)
        {
            var medicalRecords = await _unitOfWork.GetRepository<MedicalRecord>()
                .FindAsync(mr => mr.VisitDate >= request.DateRangeStart &&
                                mr.VisitDate <= request.DateRangeEnd,
                          include: q => q.Include(mr => mr.Patient)
                                        .Include(mr => mr.Appointment));

            content.AppendLine("MEDICAL ACTIVITY REPORT");
            content.AppendLine("=======================");
            content.AppendLine($"Total Patient Visits: {medicalRecords.Count()}");
            content.AppendLine($"Unique Patients: {medicalRecords.Select(mr => mr.PatientId).Distinct().Count()}");
            content.AppendLine();

            // Group by patient
            var byPatient = medicalRecords.GroupBy(mr => mr.Patient);
            foreach (var group in byPatient)
            {
                content.AppendLine($"Patient: {group.Key.FullName}");
                content.AppendLine($"Total Visits: {group.Count()}");
                content.AppendLine("Visits:");
                foreach (var record in group)
                {
                    content.AppendLine($"- {record.VisitDate:d}: {record.Prescriptions}");
                }
                content.AppendLine();
            }
        }

        private async Task GenerateAppointmentSummary(GenerateReportRequest request, StringBuilder content)
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .FindAsync(a => a.DateTime >= request.DateRangeStart &&
                               a.DateTime <= request.DateRangeEnd,
                          include: q => q.Include(a => a.Patient)
                                        .Include(a => a.Doctor));

            content.AppendLine("APPOINTMENT SUMMARY");
            content.AppendLine("===================");
            content.AppendLine($"Total Appointments: {appointments.Count()}");
            content.AppendLine();

            // Status breakdown
            content.AppendLine("STATUS BREAKDOWN");
            content.AppendLine("----------------");
            var statusGroups = appointments.GroupBy(a => a.Status);
            foreach (var group in statusGroups)
            {
                content.AppendLine($"{group.Key}: {group.Count()} appointments");
            }
            content.AppendLine();

            // Daily schedule
            content.AppendLine("DAILY SCHEDULE");
            content.AppendLine("--------------");
            var dailyGroups = appointments.GroupBy(a => a.DateTime.Date).OrderBy(g => g.Key);
            foreach (var day in dailyGroups)
            {
                content.AppendLine($"{day.Key:d}");
                foreach (var appt in day.OrderBy(a => a.DateTime))
                {
                    content.AppendLine($"- {appt.DateTime:t}: {appt.Patient.FullName} with Dr. {appt.Doctor.FullName}");
                }
                content.AppendLine();
            }
        }

        private async Task GenerateBillingHistory(GenerateReportRequest request, StringBuilder content)
        {
            var bills = await _unitOfWork.GetRepository<Billing>()
                .FindAsync(b => b.InvoiceDate >= request.DateRangeStart &&
                               b.InvoiceDate <= request.DateRangeEnd,
                          include: q => q.Include(b => b.Patient)
                                        .Include(b => b.Appointment));

            content.AppendLine("BILLING HISTORY");
            content.AppendLine("===============");
            content.AppendLine($"Total Bills Generated: {bills.Count()}");
            content.AppendLine($"Total Amount Billed: {bills.Sum(b => b.Payments.Sum(p=>p.Amount)):C}");
            content.AppendLine($"Total Amount Paid: {bills.Sum(b=>b.TotalCost):C}");
            content.AppendLine();

            // Detailed billing records
            content.AppendLine("BILLING RECORDS");
            content.AppendLine("---------------");
            foreach (var bill in bills.OrderBy(b => b.InvoiceDate))
            {
                content.AppendLine($"{bill.InvoiceDate:d} - {bill.Patient.FullName}");
                content.AppendLine($"- Amount: {bill.TotalCost:C}");
                content.AppendLine($"- Status: {(bill.IsPaid ? "Paid" : "Pending")}");
                content.AppendLine($"- Appointment: {bill.Appointment?.DateTime:d}");
                content.AppendLine();
            }
        }
        #endregion
    }
}
