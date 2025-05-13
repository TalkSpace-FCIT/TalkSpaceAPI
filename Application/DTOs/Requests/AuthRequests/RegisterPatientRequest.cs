using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests.AuthRequests
{
    public class RegisterPatientRequest
    {
        [Required(ErrorMessage = "Full name is required.")]
        public required string FullName { get; init; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public required string Password { get; init; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords must match.")]
        public required string ConfirmPassword { get; init; }
    }
}
