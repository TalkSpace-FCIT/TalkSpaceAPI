namespace Application.DTOs.Responses.AuthResponse
{
    public record ProfileResponse(
        UserResponse User,
        DateTime LastUpdated);
}
