namespace Application.DTOs.Responses.AuthResponse
{
    public record UpdateProfileResponse(
        UserResponse UpdatedProfile,
        string Message = "Profile updated successfully.");

}
