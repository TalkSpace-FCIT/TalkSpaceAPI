namespace Domain.Entities
{
    public class MedicalRecord
    {
        public DateTime VisitDate { get; set; }
        public string Notes { get; set; }
        public string Prescriptions { get; set; }

        // Foreign Keys
        public int AppointmentId { get; set; }
       public int PatientId { get; set; }
        public int DoctorId { get; set; }

        // Navigation Properties
        public Appointment Appointment { get; set; }
        // uncomment when adding theri classes
        //public Patient Patient { get; set; }
        //public Doctor Doctor { get; set; }
    }
}