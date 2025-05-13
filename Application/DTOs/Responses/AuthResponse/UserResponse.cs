namespace Application.DTOs.Responses.AuthResponse
{
    public record UserResponse(
    string UserId,
    string FullName,
    string Email,
    string? Bio,
    string Role,
    string Token);
}
