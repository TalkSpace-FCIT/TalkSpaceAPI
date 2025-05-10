using Domain.Enums;

namespace Domain.Entities
{
        public class Billing
        {
            public List<string> ServicesRendered { get; set; } // EF Core will serialize as JSON
            public decimal TotalCost { get; set; }
            public DateTime InvoiceDate { get; set; }
            public PaymentStatus Status { get; set; } // Enum: Paid, Unpaid, PartiallyPaid

            // Foreign Key
            public int PatientId { get; set; }

        // Navigation Properties
        //uncomment when adding theri classes
        // public Patient Patient { get; set; }
        public ICollection<Payment> Payments { get; set; }

        }

    }