using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.DTOs;
using Orders.Application.Services.Service;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Interfaces.Repository;
using TechTalk.SpecFlow;
using System.Globalization;
using System.Text;

namespace Orders.Tests.BDD.Steps;

[Binding]
public class ProcessPaymentSteps
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _paymentService;

    private Order? _existingOrder;
    private PaymentWebhookDto? _webhookDto;
    private PaymentWebhookResponseDto? _result;
    private Guid _currentOrderId;

    public ProcessPaymentSteps()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _paymentService = new PaymentService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Given(@"an order with id (.*) exists with status ""(.*)""")]
    public void GivenAnOrderExistsWithStatus(int orderNumber, string status)
    {
        var orderStatus = Enum.Parse<OrderStatusEnum>(status);
        _currentOrderId = Guid.NewGuid();

        _existingOrder = new Order
        {
            Id = _currentOrderId,
            Number = orderNumber * 100,
            Status = orderStatus,
            PaymentStatus = PaymentStatusEnum.PENDING,
            Items = new List<OrderItem>()
        };

        _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(_currentOrderId))
            .ReturnsAsync(_existingOrder);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(_existingOrder);
    }

    [Given(@"an order with id (.*) exists with paymentId ""(.*)"" and status ""(.*)""")]
    public void GivenAnOrderExistsWithPaymentIdAndStatus(int orderNumber, string paymentId, string status)
    {
        var paymentStatus = Enum.Parse<PaymentStatusEnum>(status);
        _currentOrderId = Guid.NewGuid();

        _existingOrder = new Order
        {
            Id = _currentOrderId,
            Number = orderNumber * 100,
            Status = OrderStatusEnum.IN_PREPARATION,
            PaymentId = paymentId,
            PaymentStatus = paymentStatus,
            Items = new List<OrderItem>()
        };

        _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(_currentOrderId))
            .ReturnsAsync(_existingOrder);
    }

    [When(@"I receive a webhook with status ""(.*)"" and paymentId ""(.*)""")]
    public async Task WhenIReceiveAWebhook(string status, string paymentId)
    {
        _webhookDto = new PaymentWebhookDto(
            Status: status,
            OrderId: _existingOrder!.Id.ToString(),
            PaymentId: paymentId
        );

        _result = await _paymentService.ProcessWebhookAsync(_webhookDto);
    }

    [When(@"I receive a webhook with invalid orderId ""(.*)""")]
    public async Task WhenIReceiveAWebhookWithInvalidOrderId(string invalidOrderId)
    {
        _webhookDto = new PaymentWebhookDto(
            Status: "PAID",
            OrderId: invalidOrderId,
            PaymentId: "pay_123"
        );

        _result = await _paymentService.ProcessWebhookAsync(_webhookDto);
    }

    [When(@"I receive a webhook for non-existent order ""(.*)""")]
    public async Task WhenIReceiveAWebhookForNonExistentOrder(string orderId)
    {
        // Parse do Guid recebido
        var nonExistentOrderId = Guid.Parse(orderId);

        _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(nonExistentOrderId))
            .ReturnsAsync((Order?)null);

        _webhookDto = new PaymentWebhookDto(
            Status: "PAID",
            OrderId: orderId, // Usar o Guid recebido como string
            PaymentId: "pay_123"
        );

        _result = await _paymentService.ProcessWebhookAsync(_webhookDto);
    }

    [When(@"I receive a duplicate webhook with paymentId ""(.*)""")]
    public async Task WhenIReceiveADuplicateWebhook(string paymentId)
    {
        _webhookDto = new PaymentWebhookDto(
            Status: "PAID",
            OrderId: _existingOrder!.Id.ToString(),
            PaymentId: paymentId
        );

        _result = await _paymentService.ProcessWebhookAsync(_webhookDto);
    }

    [Then(@"the payment should be processed successfully")]
    public void ThenThePaymentShouldBeProcessedSuccessfully()
    {
        _result.Should().NotBeNull();
        _result!.Success.Should().BeTrue();
    }

    [Then(@"the webhook processing should fail")]
    public void ThenTheWebhookProcessingShouldFail()
    {
        _result.Should().NotBeNull();
        _result!.Success.Should().BeFalse();
    }

    [Then(@"the order status should be ""(.*)""")]
    public void ThenTheOrderStatusShouldBe(string expectedStatus)
    {
        var status = Enum.Parse<OrderStatusEnum>(expectedStatus);
        _existingOrder!.Status.Should().Be(status);
    }

    [Then(@"the order status should remain ""(.*)""")]
    public void ThenTheOrderStatusShouldRemain(string expectedStatus)
    {
        var status = Enum.Parse<OrderStatusEnum>(expectedStatus);
        _existingOrder!.Status.Should().Be(status);
    }

    [Then(@"the order payment status should be ""(.*)""")]
    public void ThenTheOrderPaymentStatusShouldBe(string expectedStatus)
    {
        var status = Enum.Parse<PaymentStatusEnum>(expectedStatus);
        _existingOrder!.PaymentStatus.Should().Be(status);
    }

    [Then(@"the order should have paymentId ""(.*)""")]
    public void ThenTheOrderShouldHavePaymentId(string expectedPaymentId)
    {
        _existingOrder!.PaymentId.Should().Be(expectedPaymentId);
    }

    [Then(@"the error message should contain ""(.*)""")]
    public void ThenTheErrorMessageShouldContain(string expectedMessage)
    {
        _result.Should().NotBeNull();

        var normalizedMessage = RemoveAccents(_result!.Message);
        var normalizedExpected = RemoveAccents(expectedMessage);

        normalizedMessage.Should().Contain(normalizedExpected);
    }

    [Then(@"the message should contain ""(.*)""")]
    public void ThenTheMessageShouldContain(string expectedMessage)
    {
        _result.Should().NotBeNull();

        var normalizedMessage = RemoveAccents(_result!.Message);
        var normalizedExpected = RemoveAccents(expectedMessage);

        normalizedMessage.Should().Contain(normalizedExpected);
    }

    [Then(@"the order should not be updated again")]
    public void ThenTheOrderShouldNotBeUpdatedAgain()
    {
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    private static string RemoveAccents(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}