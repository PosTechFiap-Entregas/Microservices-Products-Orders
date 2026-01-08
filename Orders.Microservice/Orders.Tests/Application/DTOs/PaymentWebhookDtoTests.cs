using FluentAssertions;
using Orders.Application.DTOs;

namespace Orders.Tests.Application.DTOs
{
    public class PaymentWebhookDtoTests
    {
        [Fact]
        public void PaymentWebhookDto_WithValidData_ShouldCreateSuccessfully()
        {
            var status = "PAID";
            var orderId = "123";
            var paymentId = "pay_abc123";

            var dto = new PaymentWebhookDto(status, orderId, paymentId);

            dto.Should().NotBeNull();
            dto.Status.Should().Be(status);
            dto.OrderId.Should().Be(orderId);
            dto.PaymentId.Should().Be(paymentId);
        }

        [Theory]
        [InlineData("PAID")]
        [InlineData("REFUSED")]
        [InlineData("CANCELLED")]
        public void PaymentWebhookDto_WithDifferentStatuses_ShouldCreateSuccessfully(string status)
        {
            var dto = new PaymentWebhookDto(status, "1", "pay_123");

            dto.Status.Should().Be(status);
        }

        [Fact]
        public void PaymentWebhookDto_WithGuidOrderId_ShouldCreateSuccessfully()
        {
            var orderId = Guid.NewGuid().ToString();

            var dto = new PaymentWebhookDto("PAID", orderId, "pay_123");

            dto.OrderId.Should().Be(orderId);
        }

        [Fact]
        public void PaymentWebhookDto_WithIntOrderId_ShouldCreateSuccessfully()
        {
            var orderId = "999";

            var dto = new PaymentWebhookDto("PAID", orderId, "pay_123");

            dto.OrderId.Should().Be(orderId);
        }

        [Fact]
        public void PaymentWebhookDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var dto1 = new PaymentWebhookDto("PAID", "1", "pay_123");
            var dto2 = new PaymentWebhookDto("PAID", "1", "pay_123");

            dto1.Should().Be(dto2);
        }
    }
}