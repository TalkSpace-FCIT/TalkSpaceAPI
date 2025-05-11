namespace Domain.Entities
{
    public class Patient : AppUser
    {
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }

        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    }
}
