using Domain.Enums;

namespace Domain.Entities
{
    public class Payment
    {
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } // Enum: Cash, CreditCard, Insurance
        public PaymentResult Status { get; set; } // Enum: Success, Pending, Refunded

        // Foreign Key
        public int BillingId { get; set; }

        // Navigation Property
        public Billing Billing { get; set; }
    }



}