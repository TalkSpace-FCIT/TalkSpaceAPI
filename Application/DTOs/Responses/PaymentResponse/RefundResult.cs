using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.PaymentResponse
{
public class RefundResult
    {
        public string RefundId { get; set; }          // Stripe refund ID (e.g., "re_123")
        public string PaymentIntentId { get; set; }   // Original payment reference
        public string SessionId { get; set; }        // Your TalkSpace session identifier
        public decimal AmountRefunded { get; set; }   // Actual refunded amount
        public string Currency { get; set; }         // e.g., "USD"
        public string Status { get; set; }           // "succeeded", "pending", "failed"
        public string Reason { get; set; }           // "patientRequest", "doctorCancellation", etc.
        public DateTimeOffset Created { get; set; }   // When refund was processed
        public string ReceiptUrl { get; set; }       // Stripe refund receipt URL
        public string CancellationNote { get; set; } // Optional patient/doctor note

        // Helper properties
        public bool IsSuccessful => Status == "succeeded";
        public string FormattedAmount => $"{AmountRefunded} {Currency}";
    }
}
