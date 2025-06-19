
using Domain.Interfaces;
using Persistence.Context;
using System.Collections.Concurrent;

namespace Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private bool _disposed;
        private readonly ConcurrentDictionary<Type, object> _repositories;

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _disposed = false;
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_dbContext));
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {

                var errorMessages = new List<string>();
                var current = ex;
                while (current != null)
                {
                    errorMessages.Add(current.Message);
                    current = current.InnerException;
                }

                var fullError = string.Join(" --> ", errorMessages);
                Console.WriteLine($"SaveChanges failed: {fullError}");

                // Option 2: Re-throw with combined error information
                throw new Exception($"Error while saving changes: {fullError}", ex);
            }
        }


        public async Task RollbackAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
