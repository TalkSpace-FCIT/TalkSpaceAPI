using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IJWtTokenService
    {
        Task<string> GenerateTokenAsync(AppUser user, CancellationToken cancellationToken = default);
    }
}
