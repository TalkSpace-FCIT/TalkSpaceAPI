using Application.Abstractions;
using Application.DTOs.Requests.PaymentRequests;
using Application.Services;
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
        private readonly IpaymentService paymentService;

        public PaymentController(IpaymentService _paymentService)
        {
            paymentService = _paymentService;
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
                    Status = "Payment intent created successfully"
                });

                
            }
            catch (StripeException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, ex.StripeError?.Message ?? "Payment processing error");
            }
            catch (Exception ex)
            {
                // Log the exception (for development)
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }


        }

    }
}
