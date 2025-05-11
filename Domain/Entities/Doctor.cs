namespace Domain.Entities
{
    public class Doctor : AppUser
    {
        public string Specialization { get; set; } = null!;
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    }
}
