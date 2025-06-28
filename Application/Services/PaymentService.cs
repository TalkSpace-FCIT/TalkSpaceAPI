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
using Stripe.V2;
using Microsoft.Extensions.Configuration;


namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IpaymentRepository paymentrepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly IBillingRepository _billingRepository;
        private readonly IConfiguration configuration;
        private readonly IStripeHelperService _stripeHelperService;

        public PaymentService(
            IpaymentRepository paymentrepository, 
            IUnitOfWork unitOfWork,
            ILogger<PaymentService> logger,
            IBillingRepository billingRepository,
            IConfiguration _configuration,
            IStripeHelperService stripeHelperService
            )
        {
            this.paymentrepository = paymentrepository;
            this.unitOfWork = unitOfWork;
            _logger = logger;
            _billingRepository = billingRepository;
            configuration = _configuration;
            _stripeHelperService = stripeHelperService;
        }

        public async Task HandlePaymentConfirmationAsync(string json, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    _logger.LogError("Received empty JSON payload for Stripe webhook");
                    throw new ArgumentException("JSON payload cannot be null or empty", nameof(json));
                }

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogError("Received empty Stripe signature for webhook");
                    throw new ArgumentException("Signature cannot be null or empty", nameof(signature));
                }

                // Get webhook secret from configuration
                string endpointSecret = configuration["Stripe:WebhookSecret"];
                if (string.IsNullOrEmpty(endpointSecret))
                {
                    _logger.LogError("Stripe webhook secret is not configured");
                    throw new InvalidOperationException("Webhook secret is not configured");
                }

                // Fix: Pass the correct parameters in the right order
                var stripeEvent = _stripeHelperService.PrepareStripeEvent(json, signature, endpointSecret);

                _logger.LogInformation("Processing webhook event: {EventType} with ID: {EventId}",
                    stripeEvent.Type, stripeEvent.Id);

                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case "PaymentIntentSucceeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;
                    case "ChargeSucceeded":
                        await HandleChargeSucceeded(stripeEvent);
                        break;
                    default:
                        _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                        break;
                }
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx, "Stripe webhook signature verification failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook event");
                throw;
            }
        }
        private async Task HandleChargeSucceeded(Stripe.Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null)
            {
                _logger.LogWarning("Charge object not found in webhook event");
                return;
            }

            // Get the PaymentIntent ID from the charge
            var paymentIntentId = charge.PaymentIntentId;
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                _logger.LogWarning("PaymentIntentId not found in charge object");
                return;
            }

            var payments = await paymentrepository.FindAsync(p => p.PaymentIntentId == paymentIntentId);
            var payment = payments.FirstOrDefault();

            if (payment != null && payment.PaymentStatus == PaymentStatus.Pending)
            {
                payment.PaymentStatus = PaymentStatus.Success;
                payment.PaymentDate = DateTime.UtcNow;
                paymentrepository.Update(payment);
                await unitOfWork.CommitAsync();

                _logger.LogInformation("Payment status updated to Success for PaymentIntentId {PaymentIntentId} via charge.succeeded",
                    paymentIntentId);
            }
            else if (payment == null)
            {
                _logger.LogWarning("No payment record found for PaymentIntentId {PaymentIntentId} from charge",
                    paymentIntentId);
            }
            else
            {
                _logger.LogInformation("Payment {PaymentId} already processed with status {Status}",
                    payment.Id, payment.PaymentStatus);
            }
        }

        private async Task HandlePaymentIntentSucceeded(Stripe.Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogWarning("PaymentIntent object not found in webhook event");
                return;
            }

            // Find the payment record by PaymentIntentId
            var payments = await paymentrepository.FindAsync(p => p.PaymentIntentId == paymentIntent.Id);
            var payment = payments.FirstOrDefault();
            _logger.LogDebug("Payment has Allocated by PaymentIntentID {paymentIntent} ", paymentIntent.Id);
            _logger.LogDebug("the payment json ",payment.ToString());

            if (payment != null && payment.PaymentStatus == PaymentStatus.Pending)
            {
                _logger.LogDebug("Payment is not null and the status of it is Pending");
                payment.PaymentStatus = PaymentStatus.Success;
                payment.PaymentDate = DateTime.UtcNow;
                paymentrepository.Update(payment);
                await unitOfWork.CommitAsync();

                _logger.LogInformation("Payment status updated to Success for PaymentIntentId {PaymentIntentId}",
                    paymentIntent.Id);
            }
            else if (payment == null)
            {
                _logger.LogWarning("No payment record found for PaymentIntentId {PaymentIntentId}",
                    paymentIntent.Id);
            }
            else
            {
                _logger.LogInformation("Payment {PaymentId} already processed with status {Status}",
                    payment.Id, payment.PaymentStatus);
            }
        }


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
