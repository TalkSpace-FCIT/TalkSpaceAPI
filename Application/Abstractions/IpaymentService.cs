using Application.DTOs.Requests.PaymentRequests;
using Application.DTOs.Responses.PaymentResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IPaymentService
    {
        // Core session payment
        Task<SessionBookingRequest> PayForSessionAsync(SessionBookingRequest request);

        // Refund for cancellation
        Task<RefundResult> RequestSessionRefundAsync(string sessionId, string reason);

        // Webhook for payment confirmation
        Task HandlePaymentConfirmationAsync(string json, string signature);
    }
}
