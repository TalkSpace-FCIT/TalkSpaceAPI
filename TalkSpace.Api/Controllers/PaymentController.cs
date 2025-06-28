using Application.Abstractions;
using Application.DTOs.Requests.PaymentRequests;
using Application.Services;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace TalkSpace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService _paymentService, ILogger<PaymentController> logger)
        {
            paymentService = _paymentService;
            _logger = logger;
        }
        [HttpPost("Checkout")]
        public async Task<IActionResult> CreatePaymentSession([FromBody]SessionBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid) { 
                    return BadRequest(ModelState);
                }
                var result=await paymentService.PayForSessionAsync(request);
                return Ok(new
                {
                    sessionId = request.SessionId,
                    Amount = result.Amount,
                    Currency = result.Currency,
                    PaymentIntentId = result.PaymentIntentId,
                    ClientSecretCredential = result.ClientSecret,
                    Status = "Payment intent created successfully"
                });

                
            }
            catch (StripeException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.StripeError?.Message ?? "Payment processing error");
            }
            catch (Exception ex)
            {
                // Log the exception using the injected logger
                _logger.LogError(ex, "An unexpected error occurred while processing the payment.");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }


        }

    }
}
