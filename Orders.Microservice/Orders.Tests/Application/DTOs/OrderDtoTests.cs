using FluentAssertions;
using Orders.Application.DTOs;
using Orders.Domain.Enums;

namespace Orders.Tests.Application.DTOs
{
    public class OrderDtoTests
    {
        [Fact]
        public void OrderDto_WithValidData_ShouldCreateSuccessfully()
        {
            var id = 1;
            var customerId = 10;
            var status = OrderStatusEnum.RECEIVED;
            var observation = "Pedido teste";
            var number = 100;
            var paymentId = "pay_123";
            var paymentStatus = PaymentStatusEnum.PENDING;
            var total = 150.50m;
            var createdAt = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow;
            var items = new List<OrderItemDto>
            {
                new OrderItemDto(1, 1, "Produto 1", 2, 25.00m, 50.00m)
            };

            var dto = new OrderDto(
                id, customerId, status, observation, number,
                paymentId, paymentStatus, total, createdAt, updatedAt, items
            );

            dto.Should().NotBeNull();
            dto.Id.Should().Be(id);
            dto.CustomerId.Should().Be(customerId);
            dto.Status.Should().Be(status);
            dto.Observation.Should().Be(observation);
            dto.Number.Should().Be(number);
            dto.PaymentId.Should().Be(paymentId);
            dto.PaymentStatus.Should().Be(paymentStatus);
            dto.Total.Should().Be(total);
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
            dto.Items.Should().HaveCount(1);
        }

        [Fact]
        public void OrderDto_WithNullableFields_ShouldCreateSuccessfully()
        {
            var createdAt = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow;
            var items = new List<OrderItemDto>();

            var dto = new OrderDto(
                1, null, OrderStatusEnum.RECEIVED, null, 100,
                null, PaymentStatusEnum.PENDING, 0m, createdAt, updatedAt, items
            );

            dto.CustomerId.Should().BeNull();
            dto.Observation.Should().BeNull();
            dto.PaymentId.Should().BeNull();
        }

        [Fact]
        public void OrderDto_WithDifferentStatuses_ShouldStoreCorrectly()
        {
            var createdAt = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow;
            var items = new List<OrderItemDto>();

            var dto = new OrderDto(
                1, 1, OrderStatusEnum.FINALIZED, "Obs", 100,
                "pay_123", PaymentStatusEnum.PAID, 100m, createdAt, updatedAt, items
            );

            dto.Status.Should().Be(OrderStatusEnum.FINALIZED);
            dto.PaymentStatus.Should().Be(PaymentStatusEnum.PAID);
        }

        [Fact]
        public void OrderDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var createdAt = new DateTime(2024, 1, 1);
            var updatedAt = new DateTime(2024, 1, 2);
            var items = new List<OrderItemDto>();

            var dto1 = new OrderDto(
                1, 1, OrderStatusEnum.RECEIVED, "Obs", 100,
                "pay_123", PaymentStatusEnum.PENDING, 100m, createdAt, updatedAt, items
            );

            var dto2 = new OrderDto(
                1, 1, OrderStatusEnum.RECEIVED, "Obs", 100,
                "pay_123", PaymentStatusEnum.PENDING, 100m, createdAt, updatedAt, items
            );

            dto1.Should().Be(dto2);
        }
    }
}