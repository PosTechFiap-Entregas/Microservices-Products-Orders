using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.DTOs;
using Orders.Application.Services.Service;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Interfaces.Repository;

namespace Orders.Tests.Application.Services.Service;

public class PaymentServiceTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _service = new PaymentService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithValidPaidWebhook_ProcessesSuccessfully()
    {
        var order = new Order
        {
            Id = 1,
            Number = 100,
            Status = OrderStatusEnum.RECEIVED,
            PaymentStatus = PaymentStatusEnum.PENDING,
            Items = new List<OrderItem>()
        };

        var webhookDto = new PaymentWebhookDto(
            Status: "PAID",
            OrderId: "1",
            PaymentId: "pay_123456"
        );

        _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(1))
            .ReturnsAsync(order);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        var result = await _service.ProcessWebhookAsync(webhookDto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("PAID");
        result.OrderNumber.Should().Be(100);

        _repositoryMock.Verify(r => r.GetByIdWithItemsAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithInvalidOrderId_ReturnsFailure()
    {
        var webhookDto = new PaymentWebhookDto(
            Status: "PAID",
            OrderId: "invalid",
            PaymentId: "pay_123"
        );

        var result = await _service.ProcessWebhookAsync(webhookDto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("OrderId inválido");

        _repositoryMock.Verify(r => r.GetByIdWithItemsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithNonExistentOrder_ReturnsFailure()
    {
        var webhookDto = new PaymentWebhookDto(
            Status: "PAID",
            OrderId: "999",
            PaymentId: "pay_123"
        );

        _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(999))
            .ReturnsAsync((Order?)null);

        var result = await _service.ProcessWebhookAsync(webhookDto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("não encontrado");

        _repositoryMock.Verify(r => r.GetByIdWithItemsAsync(999), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithDuplicateWebhook_ReturnsDuplicateMessage()
    {
        var order = new Order
        {
            Id = 1,
            Number = 100,
            PaymentId = "pay_123",
            PaymentStatus = PaymentStatusEnum.PAID,
            Items = new List<OrderItem>()
        };

        var webhookDto = new PaymentWebhookDto(
            Status: "PAID",
            OrderId: "1",
            PaymentId: "pay_123"
        );

        _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(1))
            .ReturnsAsync(order);

        var result = await _service.ProcessWebhookAsync(webhookDto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("já processado");

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Theory]
    [InlineData("REFUSED", PaymentStatusEnum.REFUSED)]
    [InlineData("CANCELLED", PaymentStatusEnum.CANCELLED)]
    [InlineData("PAID", PaymentStatusEnum.PAID)]
    public async Task ProcessWebhookAsync_ConvertsStatusCorrectly(
        string webhookStatus,
        PaymentStatusEnum expectedStatus)
    {
        var order = new Order
        {
            Id = 1,
            Number = 100,
            Status = OrderStatusEnum.RECEIVED,
            Items = new List<OrderItem>()
        };

        var webhookDto = new PaymentWebhookDto(
            Status: webhookStatus,
            OrderId: "1",
            PaymentId: "pay_123"
        );

        _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(1))
            .ReturnsAsync(order);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        var result = await _service.ProcessWebhookAsync(webhookDto);

        result.Success.Should().BeTrue();
        order.PaymentStatus.Should().Be(expectedStatus);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }
}