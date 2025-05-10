using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string Discriminator { get; set;} = null!;
        public string? Bio { get; set; }
    }
}
