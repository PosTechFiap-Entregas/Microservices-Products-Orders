using FluentAssertions;
using Orders.Application.DTOs;

namespace Orders.Tests.Application.DTOs
{
    public class SetPaymentIdDtoTests
    {
        [Fact]
        public void SetPaymentIdDto_WithValidPaymentId_ShouldCreateSuccessfully()
        {
            var paymentId = "pay_abc123xyz";

            var dto = new SetPaymentIdDto(paymentId);

            dto.Should().NotBeNull();
            dto.PaymentId.Should().Be(paymentId);
        }

        [Fact]
        public void SetPaymentIdDto_WithEmptyPaymentId_ShouldCreateSuccessfully()
        {
            var dto = new SetPaymentIdDto(string.Empty);

            dto.PaymentId.Should().BeEmpty();
        }

        [Fact]
        public void SetPaymentIdDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var dto1 = new SetPaymentIdDto("pay_123");
            var dto2 = new SetPaymentIdDto("pay_123");

            dto1.Should().Be(dto2);
        }

        [Fact]
        public void SetPaymentIdDto_DifferentPaymentIds_ShouldNotBeEqual()
        {
            var dto1 = new SetPaymentIdDto("pay_123");
            var dto2 = new SetPaymentIdDto("pay_456");

            dto1.Should().NotBe(dto2);
        }
    }
}