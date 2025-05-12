
namespace Application.DTOs.Responses
{
    public record UserResponse(
    string UserId,
    string FullName,
    string Email,
    string? Bio,
    string Role,
    string Token);
}
