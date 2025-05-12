
namespace Application.DTOs.Responses
{
    public record AuthResponse(UserResponse User, string Message, string Token);

}
