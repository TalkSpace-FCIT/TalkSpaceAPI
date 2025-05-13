using Application.Abstractions;
using Application.DTOs.Requests.AuthRequests;
using Application.DTOs.Responses.AuthResponse;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Results;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Security.Claims;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRepository<AppUser> _userRepository;

        public AuthService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IJwtTokenService jwtTokenService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _userRepository = unitOfWork.GetRepository<AppUser>();
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                Log.Information("Login attempt for email: {Email}", request.Email);

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    Log.Warning("Login failed - user not found for email: {Email}", request.Email);
                    return Result<AuthResponse>.Failure("Invalid credentials", ErrorSource.TalkSpaceAPI);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    Log.Warning("Login failed - invalid password for user: {UserId}", user.Id);
                    return Result<AuthResponse>.Failure("Invalid credentials", ErrorSource.TalkSpaceAPI);
                }

                var token = await _jwtTokenService.GenerateTokenAsync(user);
                var userResponse = CreateUserResponse(user) with { Token = token }; // Use 'with' expression to set the Token property

   
                Log.Information("Login successful for user: {UserId}", user.Id);
                return Result<AuthResponse>.Success(new AuthResponse(userResponse), "Login Successful");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during login for email: {Email}", request.Email);
                return Result<AuthResponse>.Failure("An error occurred during login", ErrorSource.Database);
            }
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterPatientRequest request)
        {
            try
            {
                Log.Information("Registration attempt for email: {Email}", request.Email);

                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    Log.Warning("Registration failed - email already exists: {Email}", request.Email);
                    return Result<AuthResponse>.Failure("Email already exists", ErrorSource.TalkSpaceAPI);
                }

                var user = new Patient
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    UserName = request.Email
                };

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    Log.Warning("User creation failed - errors: {Errors}", errors);
                    return Result<AuthResponse>.Failure(errors, ErrorSource.TalkSpaceAPI);
                }

                await _userManager.AddToRoleAsync(user, "Patient");
                var token = await _jwtTokenService.GenerateTokenAsync(user);
                var userResponse = CreateUserResponse(user) with {Token = token };

                Log.Information("Registration successful for user: {UserId}", user.Id);
                return Result<AuthResponse>.Success(new AuthResponse(userResponse), "Successful registration.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during registration for email: {Email}", request.Email);
                return Result<AuthResponse>.Failure("An error occurred during registration", ErrorSource.Database);
            }
        }

        public async Task<Result<LogoutResponse>> LogoutAsync(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    Log.Warning("Logout failed - user not found in claims");
                    return Result<LogoutResponse>.Failure("User not found", ErrorSource.TalkSpaceAPI);
                }

                await _signInManager.SignOutAsync();
                Log.Information("Logout successful for user: {UserId}", userId);
                return Result<LogoutResponse>.Success(new LogoutResponse { }, "logout successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during logout");
                return Result<LogoutResponse>.Failure("An error occurred during logout", ErrorSource.Database);
            }
        }

        public async Task<Result<ProfileResponse>> GetProfileAsync(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var userEmail = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userEmail == null)
                {
                    Log.Warning("GetProfile failed - user not found in claims");
                    return Result<ProfileResponse>.Failure("User not found", ErrorSource.TalkSpaceAPI);
                }

                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    Log.Warning("GetProfile failed - user not found in database: {userEmail}", userEmail);
                    return Result<ProfileResponse>.Failure("User not found", ErrorSource.TalkSpaceAPI);
                }

                Log.Information("Profile retrieved successfully for user: {userEmail}", userEmail);
                return Result<ProfileResponse>.Success(new ProfileResponse(
                    User: CreateUserResponse(user),
                    LastUpdated: DateTime.UtcNow
                ),
                "user profile retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving profile");
                return Result<ProfileResponse>.Failure("An error occurred retrieving profile", ErrorSource.Database);
            }
        }

        public async Task<Result<UpdateProfileResponse>> UpdateProfileAsync(
            ClaimsPrincipal claimsPrincipal,
            UpdateProfileRequest request)
        {
            try
            {
                var userEmail = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userEmail == null)
                {
                    Log.Warning("UpdateProfile failed - user not found in claims");
                    return Result<UpdateProfileResponse>.Failure("User not found", ErrorSource.TalkSpaceAPI);
                }

                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    Log.Warning("UpdateProfile failed - user not found in database: {userEmail}", userEmail);
                    return Result<UpdateProfileResponse>.Failure("User not found", ErrorSource.TalkSpaceAPI);
                }

                // Email change validation
                if (user.Email != request.Email)
                {
                    var emailExists = await _userManager.FindByEmailAsync(request.Email);
                    if (emailExists != null)
                    {
                        Log.Warning("UpdateProfile failed - email already in use: {Email}", request.Email);
                        return Result<UpdateProfileResponse>.Failure("Email already in use", ErrorSource.TalkSpaceAPI);
                    }
                }

                // Update user properties
                user.FullName = request.FullName;
                user.Email = request.Email;
                user.UserName = request.Email;

                _userRepository.Update(user);
                await _unitOfWork.CommitAsync();

                Log.Information("Profile updated successfully for user: {userEmail}", userEmail);
                return Result<UpdateProfileResponse>.Success(new UpdateProfileResponse(
                    UpdatedProfile: CreateUserResponse(user),
                    Message: "Profile updated successfully."
                ), "");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating profile for user");
                return Result<UpdateProfileResponse>.Failure("An error occurred updating profile", ErrorSource.Database);
            }
        }

        private UserResponse CreateUserResponse(AppUser user)
        {
            return new UserResponse(
                UserId: user.Id,
                FullName: user.FullName,
                Email: user.Email!,
                Bio: user.Bio,
                Role: user.Discriminator ?? "User",
                Token: ""
            ); 
        }
    }
}