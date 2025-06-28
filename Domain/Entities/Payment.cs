using Domain.Enums;

namespace Domain.Entities
{
    public class Payment : BaseEntity
    {
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } // Enum Card
        public PaymentStatus PaymentStatus { get; set; } // Enum: Success, Pending, Refunded, Failed
        public bool IsPaid => PaymentStatus == PaymentStatus.Success;
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        // Foreign Key
        public int BillingId { get; set; }

        // Navigation Property
        public Billing Billing { get; set; }

    }

}