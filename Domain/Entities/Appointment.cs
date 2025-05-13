using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Appointment : BaseEntity
    {
        public DateTime DateTime { get; set; }
        public VisitType VisitType { get; set; }
        public AppointmentStatus Status { get; set; } // Enum: Scheduled, Cancelled, Completed
        public DateTime StatusUpdatedOn { get; set; }

        // Foreign Keys
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public int MedicalRecordId { get; set; }

        // Navigation Properties
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public MedicalRecord MedicalRecord { get; set; }

        // Status History
        public List<AppointmentStatusHistory> StatusHistory { get; set; } = new();

        public Appointment()
        {
            VisitType = VisitType.consultation;
            Status = AppointmentStatus.Scheduled;
            StatusUpdatedOn = DateTime.UtcNow;
        }

        public void UpdateStatus(AppointmentStatus newStatus)
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot change status of a completed appointment.");

            if (Status == AppointmentStatus.Cancelled && newStatus != AppointmentStatus.Scheduled)
                throw new InvalidOperationException("Cancelled appointments can only be rescheduled.");

            if (Status != newStatus)
            {
                StatusHistory.Add(new AppointmentStatusHistory
                {
                    OldStatus = Status,
                    NewStatus = newStatus,
                    ChangedOn = DateTime.UtcNow
                });

                Status = newStatus;
                StatusUpdatedOn = DateTime.UtcNow;
            }
        }

        public bool IsCancellable()
        {
            return Status == AppointmentStatus.Scheduled;
        }
    }

    // Owned by Appointment.
    public class AppointmentStatusHistory
    {
        public AppointmentStatus OldStatus { get; set; }
        public AppointmentStatus NewStatus { get; set; }
        public DateTime ChangedOn { get; set; }
    }
}
