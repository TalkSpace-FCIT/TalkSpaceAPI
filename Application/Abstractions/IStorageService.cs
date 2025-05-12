namespace Application.Abstractions
{
    public interface IStorageService
    {
        Task<string> StoreReportAsync(int reportId, byte[] content);
        Task<byte[]> RetrieveReportAsync(int reportId);
    }
}
