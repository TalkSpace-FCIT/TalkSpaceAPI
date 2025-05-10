using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Appointment:BaseEntity
    {
        public DateTime DateTime { get; set; }
        public string VisitType { get; set; }
        public AppointmentStatus Status { get; set; } // Enum: Scheduled, Cancelled, Completed

        // Foreign Keys
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        // Navigation Properties

        //public Patient Patient { get; set; }
        //public Doctor Doctor { get; set; }
        public MedicalRecord MedicalRecord { get; set; }
    }

}
