using Application.Abstractions;
using Application.DTOs.Requests.PaymentRequests;
using Application.DTOs.Responses.PaymentResponse;
using Domain.Entities;
using iTextSharp.text.log;
using Stripe;
using Domain.Interfaces;
using Domain.Enums;
using Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class PaymentService : IpaymentService
    {
        private readonly IpaymentRepository paymentrepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly IBillingRepository _billingRepository;

        public PaymentService(
            IpaymentRepository paymentrepository, 
            IUnitOfWork unitOfWork,
            ILogger<PaymentService> logger,
            IBillingRepository billingRepository
            )
        {
            this.paymentrepository = paymentrepository;
            this.unitOfWork = unitOfWork;
            _logger = logger;
            _billingRepository = billingRepository;
        }
        // Pseudocode for HandlePaymentConfirmationAsync:
        // 1. Validate the Stripe webhook signature using the provided signature and secret.
        // 2. Parse the JSON payload to extract event type and data.
        // 3. If event is payment_intent.succeeded, update session/payment status in DB.
        // 4. Handle other relevant events as needed.
        // 5. Return Task.CompletedTask.

        public async Task HandlePaymentConfirmationAsync(string json, string signature)
        {
            //string endpointsecrest =""
            // Example using Stripe .NET SDK (pseudo-implementation)
            // string endpointSecret = "<your-stripe-webhook-secret>";
            // var stripeEvent = EventUtility.ConstructEvent(json, signature, endpointSecret);
            // switch (stripeEvent.Type)
            // {
            //     case Events.PaymentIntentSucceeded:
            //         // Update session/payment status in your system
            //         break;
            //     // Handle other event types as needed
            // }
            await Task.CompletedTask;
        }

        // Pseudocode for PayForSessionAsync:
        // 1. Validate the request (user/session/amount).
        // 2. Create a Stripe PaymentIntent or Checkout Session.
        // 3. Save payment/session info to DB.
        // 4. Return the updated SessionBookingRequest (optionally with payment info).

        public async Task<SessionBookingRequest> PayForSessionAsync(SessionBookingRequest request)
        {
            _logger.LogInformation("Processing payment for session {SessionId} by user {UserId}", request.SessionId, request.UserId);
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.");
            }

            try
            {
                _logger.LogInformation("Creating payment intent for session {SessionId} with amount {Amount} {Currency}", request.SessionId, request.Amount, request.Currency);
                // First, get the existing billing record
                var existingBilling = await GetBillingBySessionOrAppointmentId(request.SessionId, request.SessionId);

                if (existingBilling == null)
                {
                     throw new InvalidOperationException($"No billing record found for session {request.SessionId}");
                }

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100),
                    Currency = request.Currency.ToLower(),
                    PaymentMethod = request.PaymentMethod,
                    Metadata = new Dictionary<string, string>
            {
                { "SessionId", request.SessionId.ToString() },
                { "UserId", request.UserId.ToString() },
                { "BillingId", existingBilling.Id.ToString() } // Add billing ID to metadata
            },
                    PaymentMethodTypes = new List<string> { "card" },
                    CaptureMethod = "manual",
                    SetupFutureUsage = "off_session",
                    Confirm = false,
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                _logger.LogInformation("Payment intent created with ID {PaymentIntentId} for session {SessionId}", paymentIntent.Id, request.SessionId);
                var payment = new Payment
                {
                    Amount = request.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentStatus = PaymentStatus.Pending,
                    Method = request.PaymentMethod,
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    BillingId = existingBilling.Id // Associate with existing billing record
                };
                
                request.PaymentIntentId = paymentIntent.Id;
                request.ClientSecret = paymentIntent.ClientSecret;

                await paymentrepository.AddAsync(payment);
                await unitOfWork.CommitAsync();
                _logger.LogInformation("Payment record created for session {SessionId} with PaymentIntentId {PaymentIntentId}", request.SessionId, paymentIntent.Id);
                return request;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");
                throw;
            }
        }

        // Helper method to get existing billing record
        private async Task<Billing> GetBillingBySessionOrAppointmentId(int sessionId, int? appointmentId = null)
        {
            int searchid = appointmentId ?? sessionId;

            return await _billingRepository.GetbyAppointmentID(searchid);

        }

        // Pseudocode for RequestSessionRefundAsync:
        // 1. Lookup payment intent by sessionId.
        // 2. Create a Stripe refund for the payment intent.
        // 3. Save refund info to DB.
        // 4. Return RefundResult with details.

        public async Task<RefundResult> RequestSessionRefundAsync(string sessionId, string reason)
        {
            // Example using Stripe .NET SDK (pseudo-implementation)
            // string paymentIntentId = LookupPaymentIntentIdBySessionId(sessionId);
            // var options = new RefundCreateOptions
            // {
            //     PaymentIntent = paymentIntentId,
            //     Reason = reason
            // };
            // var service = new RefundService();
            // var refund = await service.CreateAsync(options);
            // Save refund.Id and details to your DB
            return new RefundResult
            {
                RefundId = "mock_refund_id",
                PaymentIntentId = "mock_payment_intent_id",
                SessionId = sessionId,
                AmountRefunded = 0,
                Currency = "USD",
                Status = "pending",
                Reason = reason,
                Created = DateTimeOffset.UtcNow,
                ReceiptUrl = "",
                CancellationNote = "",
            };
        }
    }
}
