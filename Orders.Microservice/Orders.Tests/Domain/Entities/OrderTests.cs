using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Xunit;

namespace Orders.Tests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void CalculateTotal_WithMultipleItems_ReturnsCorrectSum()
    {
        var order = new Order();
        order.Items.Add(new OrderItem { Quantity = 2, UnitPrice = 25.90m });
        order.Items.Add(new OrderItem { Quantity = 1, UnitPrice = 12.90m });

        var total = order.CalculateTotal();

        total.Should().Be(64.70m);
    }

    [Fact]
    public void CalculateTotal_WithNoItems_ReturnsZero()
    {
        var order = new Order();

        var total = order.CalculateTotal();

        total.Should().Be(0m);
    }

    [Fact]
    public void ChangeStatus_WithValidTransition_ChangesStatus()
    {
        var order = new Order { Status = OrderStatusEnum.RECEIVED };

        order.ChangeStatus(OrderStatusEnum.IN_PREPARATION);

        order.Status.Should().Be(OrderStatusEnum.IN_PREPARATION);
    }

    [Fact]
    public void ChangeStatus_WithInvalidTransition_ThrowsInvalidOperationException()
    {
        var order = new Order { Status = OrderStatusEnum.RECEIVED };

        Action act = () => order.ChangeStatus(OrderStatusEnum.FINALIZED);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RECEIVED*FINALIZED*");
    }

    [Theory]
    [InlineData(OrderStatusEnum.RECEIVED, OrderStatusEnum.IN_PREPARATION, true)]
    [InlineData(OrderStatusEnum.IN_PREPARATION, OrderStatusEnum.READY, true)]
    [InlineData(OrderStatusEnum.READY, OrderStatusEnum.FINALIZED, true)]
    [InlineData(OrderStatusEnum.RECEIVED, OrderStatusEnum.READY, false)]
    [InlineData(OrderStatusEnum.IN_PREPARATION, OrderStatusEnum.RECEIVED, false)]
    public void ChangeStatus_ValidatesTransitions(
        OrderStatusEnum currentStatus,
        OrderStatusEnum nextStatus,
        bool shouldSucceed)
    {
        var order = new Order { Status = currentStatus };

        Action act = () => order.ChangeStatus(nextStatus);

        if (shouldSucceed)
        {
            act.Should().NotThrow();
            order.Status.Should().Be(nextStatus);
        }
        else
        {
            act.Should().Throw<InvalidOperationException>();
            order.Status.Should().Be(currentStatus);
        }
    }

    [Fact]
    public void ProcessPayment_WithPaidStatus_UpdatesStatusToInPreparation()
    {
        var order = new Order { Status = OrderStatusEnum.RECEIVED };

        order.ProcessPayment("pay_123", PaymentStatusEnum.PAID);

        order.PaymentId.Should().Be("pay_123");
        order.PaymentStatus.Should().Be(PaymentStatusEnum.PAID);
        order.Status.Should().Be(OrderStatusEnum.IN_PREPARATION);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ProcessPayment_WithInvalidPaymentId_ThrowsArgumentException(
        string invalidPaymentId)
    {
        var order = new Order();

        Action act = () => order.ProcessPayment(invalidPaymentId, PaymentStatusEnum.PAID);

        act.Should().Throw<ArgumentException>()
            .WithMessage("PaymentId não pode ser vazio");
    }

    [Fact]
    public void AddItem_WhenOrderIsReceived_AddsItemSuccessfully()
    {
        var order = new Order
        {
            Status = OrderStatusEnum.RECEIVED,
            TotalAmount = 0m
        };
        var item = new OrderItem
        {
            ProductId = 1,
            Quantity = 2,
            UnitPrice = 25.90m
        };

        order.AddItem(item);

        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(51.80m);
    }

    [Fact]
    public void AddItem_WhenOrderIsNotReceived_ThrowsInvalidOperationException()
    {
        var order = new Order { Status = OrderStatusEnum.IN_PREPARATION };
        var item = new OrderItem();

        Action act = () => order.AddItem(item);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*adicionar itens*");
    }

    [Fact]
    public void RecalculateTotal_UpdatesTotalAmountCorrectly()
    {
        var order = new Order { TotalAmount = 0m };
        order.Items.Add(new OrderItem { Quantity = 3, UnitPrice = 10.00m });
        order.Items.Add(new OrderItem { Quantity = 2, UnitPrice = 5.50m });

        order.RecalculateTotal();

        order.TotalAmount.Should().Be(41.00m);
    }
}