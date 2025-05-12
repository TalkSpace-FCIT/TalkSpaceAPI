
using Application.Abstractions;
using Serilog;
namespace Application.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _storagePath;
        public LocalStorageService(string storagePath)
        {
            _storagePath = storagePath;

            // Ensure directory exists. 
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }
        public async Task<byte[]> RetrieveReportAsync(int reportId)
        {
            try
            {
                var searchPattern = $"report_{reportId}_*.pdf";
                var files = Directory.GetFiles(_storagePath, searchPattern);

                if (files.Length == 0)
                {
                    Log.Warning("No report found for ID: {ReportId}", reportId);
                    throw new FileNotFoundException($"Report {reportId} not found");
                }

                // Get most recent version. 
                var latestFile = files.OrderByDescending(f => f).First();
                Log.Information("Retrieving report from {FilePath}", latestFile);

                return await File.ReadAllBytesAsync(latestFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving report {ReportId}", reportId);
                throw;
            }
        }

        public async Task<string> StoreReportAsync(int reportId, byte[] content)
        {
            try
            {
                var fileName = $"report_{reportId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(_storagePath, fileName);

                Log.Information("Storing report {ReportId} at {FilePath}", reportId, filePath);

                await File.WriteAllBytesAsync(filePath, content);

                Log.Debug("Successfully stored report");
                return $"/reports/{fileName}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error storing report {ReportId}", reportId);
                throw;
            }
        }
    }
}
