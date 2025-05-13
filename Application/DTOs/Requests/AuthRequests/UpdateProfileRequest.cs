using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests.AuthRequests
{
    public record UpdateProfileRequest(
        [Required(ErrorMessage = "Full name is required.")]
        string FullName,

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email
    );
}
