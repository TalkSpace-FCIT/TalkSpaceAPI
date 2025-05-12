namespace Domain.Entities
{
    public class MedicalRecord : BaseEntity
    {
        public DateTime VisitDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public string Prescriptions { get; set; } = null!;

        // Foreign Keys
        public int AppointmentId { get; set; }
        public string PatientId { get; set; } = null!;
        public string DoctorId { get; set; } = null!;

        // Navigation Properties
        public Appointment Appointment { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}