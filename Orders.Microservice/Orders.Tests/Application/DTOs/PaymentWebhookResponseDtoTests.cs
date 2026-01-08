using FluentAssertions;
using Orders.Application.DTOs;

namespace Orders.Tests.Application.DTOs
{
    public class PaymentWebhookResponseDtoTests
    {
        [Fact]
        public void PaymentWebhookResponseDto_WithSuccessAndOrderNumber_ShouldCreateSuccessfully()
        {
            var success = true;
            var message = "Pagamento processado com sucesso";
            var orderNumber = 100;

            var dto = new PaymentWebhookResponseDto(success, message, orderNumber);

            dto.Should().NotBeNull();
            dto.Success.Should().BeTrue();
            dto.Message.Should().Be(message);
            dto.OrderNumber.Should().Be(orderNumber);
        }

        [Fact]
        public void PaymentWebhookResponseDto_WithoutOrderNumber_ShouldCreateSuccessfully()
        {
            var dto = new PaymentWebhookResponseDto(true, "Mensagem");

            dto.Success.Should().BeTrue();
            dto.Message.Should().Be("Mensagem");
            dto.OrderNumber.Should().BeNull();
        }

        [Fact]
        public void PaymentWebhookResponseDto_WithFailure_ShouldCreateSuccessfully()
        {
            var success = false;
            var message = "Pedido não encontrado";

            var dto = new PaymentWebhookResponseDto(success, message, null);

            dto.Success.Should().BeFalse();
            dto.Message.Should().Be(message);
            dto.OrderNumber.Should().BeNull();
        }

        [Fact]
        public void PaymentWebhookResponseDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var dto1 = new PaymentWebhookResponseDto(true, "Sucesso", 100);
            var dto2 = new PaymentWebhookResponseDto(true, "Sucesso", 100);

            dto1.Should().Be(dto2);
        }

        [Fact]
        public void PaymentWebhookResponseDto_DifferentMessages_ShouldNotBeEqual()
        {
            var dto1 = new PaymentWebhookResponseDto(true, "Mensagem 1", 100);
            var dto2 = new PaymentWebhookResponseDto(true, "Mensagem 2", 100);

            dto1.Should().NotBe(dto2);
        }
    }
}