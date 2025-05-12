
namespace Application.DTOs.Responses
{
    public record UpdateProfileResponse(
        UserResponse UpdatedProfile,
        string Message = "Profile updated successfully.");

}
