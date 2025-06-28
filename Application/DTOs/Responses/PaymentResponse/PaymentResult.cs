using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.PaymentResponse
{
    internal class PaymentResult
    {
        public string PaymentIntentId { get; set; }     // Stripe reference
        public string ClientSecret { get; set; }        // For client-side confirmation
        public string SessionStatus { get; set; }       // "Booked", "PendingPayment"
        public string ReceiptUrl { get; set; }
    }
}
