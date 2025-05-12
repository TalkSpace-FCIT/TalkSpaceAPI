
namespace Application.DTOs.Responses
{
    public record ProfileResponse(
        UserResponse User,
        DateTime LastUpdated);
}
