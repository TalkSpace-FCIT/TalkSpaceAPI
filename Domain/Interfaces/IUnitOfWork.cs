namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>() where T : class;
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync();
    }
}
