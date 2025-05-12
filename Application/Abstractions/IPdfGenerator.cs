namespace Application.Abstractions
{
    public interface IPdfGenerator
    {
        Task<byte[]> GeneratePdfAsync(string content);
    }
}
