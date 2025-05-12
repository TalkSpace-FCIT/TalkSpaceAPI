using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(AppUser user, CancellationToken cancellationToken = default);
    }

}
