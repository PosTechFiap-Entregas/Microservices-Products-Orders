using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orders.Application.DTOs;
using Orders.Application.Services.Interface;
using System;
using System.Threading.Tasks;

namespace Orders.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IPaymentService paymentService,
            ILogger<WebhookController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PaymentWebhookResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentWebhookResponseDto>> ProcessPayment(
            [FromBody] PaymentWebhookDto webhookDto)
        {
            _logger.LogInformation(
                "Webhook recebido. OrderId: {OrderId}, Status: {Status}",
                webhookDto.OrderId, webhookDto.Status);

            if (string.IsNullOrWhiteSpace(webhookDto.OrderId))
            {
                return BadRequest(new PaymentWebhookResponseDto(
                    Success: false,
                    Message: "OrderId é obrigatório"
                ));
            }

            if (string.IsNullOrWhiteSpace(webhookDto.PaymentId))
            {
                return BadRequest(new PaymentWebhookResponseDto(
                    Success: false,
                    Message: "PaymentId é obrigatório"
                ));
            }

            if (string.IsNullOrWhiteSpace(webhookDto.Status))
            {
                return BadRequest(new PaymentWebhookResponseDto(
                    Success: false,
                    Message: "Status é obrigatório"
                ));
            }

            var result = await _paymentService.ProcessWebhookAsync(webhookDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new { status = "Webhook is healthy", timestamp = DateTime.UtcNow });
        }
    }
}