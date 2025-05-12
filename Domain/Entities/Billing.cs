using Domain.Enums;

namespace Domain.Entities
{
    public class Billing : BaseEntity
    {
        // Properties
        public decimal TotalCost => Payments.Where(p => p.PaymentStatus == PaymentStatus.Success).Sum(p => p.Amount);
        public DateTime InvoiceDate { get; set; }
        public PaymentStatus Status { get; set; } // Enum: Paid, Pending, Failed, Refunded
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        // Required Relationship: Patient (1-to-1)
        public string PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        // Optional Relationship: Payments (1-to-Many)
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public bool IsPaid => Status == PaymentStatus.Success;

        public void MarkAsPaid()
        {
            if (TotalCost > 0)
            {
                Status = PaymentStatus.Success;
            }
            else
            {
                Status = PaymentStatus.Failed;
            }
        }
    }

}