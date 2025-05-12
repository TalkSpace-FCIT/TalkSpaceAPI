using Domain.Enums;

namespace Domain.Entities
{
    public class Payment : BaseEntity
    {
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } // Enum CreditCard
        public PaymentStatus PaymentStatus { get; set; } // Enum: Success, Pending, Refunded, Failed
        public bool IsPaid => PaymentDate != DateTime.MinValue;
        // Foreign Key
        public int BillingId { get; set; }

        // Navigation Property
        public Billing Billing { get; set; }
    }



}