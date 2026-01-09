using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.API.Controllers;
using Orders.Application.DTOs;
using Orders.Application.Services.Interface;

namespace Orders.Tests.API.Controllers
{
    public class WebhookControllerTests
    {
        private readonly Mock<IPaymentService> _serviceMock;
        private readonly Mock<ILogger<WebhookController>> _loggerMock;
        private readonly WebhookController _controller;

        public WebhookControllerTests()
        {
            _serviceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<WebhookController>>();
            _controller = new WebhookController(_serviceMock.Object, _loggerMock.Object);
        }

        #region ProcessPayment Tests

        [Fact]
        public async Task ProcessPayment_WithValidData_ReturnsOkWithSuccessResponse()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "1", PaymentId: "pay_123");
            var responseDto = new PaymentWebhookResponseDto(true, "Pagamento PAID processado com sucesso", 100);

            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhookDto))
                .ReturnsAsync(responseDto);

            var result = await _controller.ProcessPayment(webhookDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResponse = okResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            returnedResponse.Should().BeEquivalentTo(responseDto);
            returnedResponse.Success.Should().BeTrue();

            _serviceMock.Verify(s => s.ProcessWebhookAsync(webhookDto), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_WithNullOrderId_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: null!, PaymentId: "pay_123");

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            response.Success.Should().BeFalse();
            response.Message.Should().Be("OrderId é obrigatório");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_WithEmptyOrderId_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "", PaymentId: "pay_123");

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            response.Success.Should().BeFalse();
            response.Message.Should().Be("OrderId é obrigatório");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_WithWhitespaceOrderId_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "   ", PaymentId: "pay_123");

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            response.Success.Should().BeFalse();
            response.Message.Should().Be("OrderId é obrigatório");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_WithNullPaymentId_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "1", PaymentId: null!);

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            response.Success.Should().BeFalse();
            response.Message.Should().Be("PaymentId é obrigatório");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_WithEmptyPaymentId_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "1", PaymentId: "");

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            response.Success.Should().BeFalse();
            response.Message.Should().Be("PaymentId é obrigatório");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_WithNullStatus_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: null!, OrderId: "1", PaymentId: "pay_123");

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Status é obrigatório");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_WithEmptyStatus_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: "", OrderId: "1", PaymentId: "pay_123");

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Status é obrigatório");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_WhenServiceReturnsFailure_ReturnsBadRequest()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "999", PaymentId: "pay_123");
            var responseDto = new PaymentWebhookResponseDto(false, "Pedido 999 não encontrado");

            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhookDto))
                .ReturnsAsync(responseDto);

            var result = await _controller.ProcessPayment(webhookDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var returnedResponse = badRequestResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            returnedResponse.Should().BeEquivalentTo(responseDto);
            returnedResponse.Success.Should().BeFalse();

            _serviceMock.Verify(s => s.ProcessWebhookAsync(webhookDto), Times.Once);
        }

        [Theory]
        [InlineData("PAID")]
        [InlineData("REFUSED")]
        [InlineData("CANCELLED")]
        [InlineData("PENDING")]
        public async Task ProcessPayment_WithDifferentStatuses_ProcessesCorrectly(string status)
        {
            var webhookDto = new PaymentWebhookDto(Status: status, OrderId: "1", PaymentId: "pay_123");
            var responseDto = new PaymentWebhookResponseDto(true, $"Pagamento {status} processado com sucesso", 100);

            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhookDto))
                .ReturnsAsync(responseDto);

            var result = await _controller.ProcessPayment(webhookDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResponse = okResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            returnedResponse.Success.Should().BeTrue();
            returnedResponse.Message.Should().Contain(status);

            _serviceMock.Verify(s => s.ProcessWebhookAsync(webhookDto), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_LogsInformation()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "1", PaymentId: "pay_123");
            var responseDto = new PaymentWebhookResponseDto(true, "Sucesso", 100);

            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhookDto))
                .ReturnsAsync(responseDto);

            await _controller.ProcessPayment(webhookDto);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Webhook recebido")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_WithMultipleValidWebhooks_ProcessesEach()
        {
            var webhook1 = new PaymentWebhookDto(Status: "PAID", OrderId: "1", PaymentId: "pay_123");
            var webhook2 = new PaymentWebhookDto(Status: "PAID", OrderId: "2", PaymentId: "pay_456");
            var webhook3 = new PaymentWebhookDto(Status: "REFUSED", OrderId: "3", PaymentId: "pay_789");

            var response1 = new PaymentWebhookResponseDto(true, "Sucesso 1", 100);
            var response2 = new PaymentWebhookResponseDto(true, "Sucesso 2", 101);
            var response3 = new PaymentWebhookResponseDto(true, "Sucesso 3", 102);

            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhook1)).ReturnsAsync(response1);
            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhook2)).ReturnsAsync(response2);
            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhook3)).ReturnsAsync(response3);

            await _controller.ProcessPayment(webhook1);
            await _controller.ProcessPayment(webhook2);
            await _controller.ProcessPayment(webhook3);

            _serviceMock.Verify(s => s.ProcessWebhookAsync(It.IsAny<PaymentWebhookDto>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ProcessPayment_WithDuplicateWebhook_ProcessesSuccessfully()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "1", PaymentId: "pay_123");
            var responseDto = new PaymentWebhookResponseDto(true, "Pagamento já processado anteriormente", 100);

            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhookDto))
                .ReturnsAsync(responseDto);

            var result = await _controller.ProcessPayment(webhookDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResponse = okResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            returnedResponse.Success.Should().BeTrue();
            returnedResponse.Message.Should().Contain("já processado");

            _serviceMock.Verify(s => s.ProcessWebhookAsync(webhookDto), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_WithOrderNumberInResponse_ReturnsOrderNumber()
        {
            var webhookDto = new PaymentWebhookDto(Status: "PAID", OrderId: "1", PaymentId: "pay_123");
            var responseDto = new PaymentWebhookResponseDto(true, "Sucesso", 100);

            _serviceMock.Setup(s => s.ProcessWebhookAsync(webhookDto))
                .ReturnsAsync(responseDto);

            var result = await _controller.ProcessPayment(webhookDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResponse = okResult.Value.Should().BeAssignableTo<PaymentWebhookResponseDto>().Subject;
            returnedResponse.OrderNumber.Should().Be(100);

            _serviceMock.Verify(s => s.ProcessWebhookAsync(webhookDto), Times.Once);
        }

        #endregion

        #region Health Tests

        [Fact]
        public void Health_ReturnsOkWithHealthStatus()
        {
            var result = _controller.Health();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();

            var healthResponse = okResult.Value;
            healthResponse.Should().NotBeNull();
        }

        [Fact]
        public void Health_ReturnsStatusProperty()
        {
            var result = _controller.Health();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var value = okResult.Value;

            value.Should().NotBeNull();
            value!.GetType().GetProperty("status").Should().NotBeNull();
        }

        [Fact]
        public void Health_ReturnsTimestampProperty()
        {
            var result = _controller.Health();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var value = okResult.Value;

            value.Should().NotBeNull();
            value!.GetType().GetProperty("timestamp").Should().NotBeNull();
        }

        [Fact]
        public void Health_CalledMultipleTimes_AlwaysReturnsOk()
        {
            var result1 = _controller.Health();
            var result2 = _controller.Health();
            var result3 = _controller.Health();

            result1.Should().BeOfType<OkObjectResult>();
            result2.Should().BeOfType<OkObjectResult>();
            result3.Should().BeOfType<OkObjectResult>();
        }

        #endregion
    }
}