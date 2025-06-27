using Application.Abstractions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace TalkSpace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhooksController : BaseApiController
    {
        private readonly IStripeHelperService _stripeHelperService;
        private readonly ILogger<StripeWebhooksController> _logger;
        private readonly IpaymentService paymentService;

        public StripeWebhooksController(
            IStripeHelperService stripeHelperService,
            ILogger<StripeWebhooksController> logger,
            IpaymentService paymentService

        )
        {
            _stripeHelperService = stripeHelperService;
            _logger = logger;
            this.paymentService = paymentService;
        }

        [HttpPost("Webhook")]
        public async Task<IActionResult> Webhook()
        {

            // Read the body first and store it
            try
            {
                // Read the body first and store it
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                if (!Request.Headers.TryGetValue("Stripe-Signature", out var stripeSignature))
                {
                    _logger.LogWarning("Stripe-Signature header not found");
                    return BadRequest("Missing Stripe-Signature header");
                }

                _logger.LogDebug("Received webhook with headers: {Headers}", Request.Headers);

                await paymentService.HandlePaymentConfirmationAsync(json, stripeSignature);

                return Ok(new { received = true });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook processing failed: {ErrorMessage}", ex.StripeError?.Message);
                return BadRequest(); // Return 400 for Stripe signature verification failures
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

