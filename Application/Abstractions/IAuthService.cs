using Application.DTOs.Responses;
using Application.DTOs;
using Domain.Results;
using System.Security.Claims;

namespace Application.Abstractions
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
        Task<Result<AuthResponse>> RegisterAsync(RegisterPatientRequest request);
        Task<Result<LogoutResponse>> LogoutAsync(ClaimsPrincipal claimsPrincipal);
        Task<Result<ProfileResponse>> GetProfileAsync(ClaimsPrincipal claimsPrincipal);
        Task<Result<UpdateProfileResponse>> UpdateProfileAsync(ClaimsPrincipal claimsPrincipal, UpdateProfileRequest request);
    }

}
