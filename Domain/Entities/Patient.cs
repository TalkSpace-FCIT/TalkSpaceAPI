namespace Domain.Entities
{
    public class Patient : AppUser
    {
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
    }
}
