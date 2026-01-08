using FluentAssertions;
using Orders.Application.DTOs;

namespace Orders.Tests.Application.DTOs
{
    public class CreateOrderItemDtoTests
    {
        [Fact]
        public void CreateOrderItemDto_WithValidData_ShouldCreateSuccessfully()
        {
            var productId = 10;
            var quantity = 5;

            var dto = new CreateOrderItemDto(productId, quantity);

            dto.Should().NotBeNull();
            dto.ProductId.Should().Be(productId);
            dto.Quantity.Should().Be(quantity);
        }

        [Fact]
        public void CreateOrderItemDto_WithZeroQuantity_ShouldCreateSuccessfully()
        {
            var dto = new CreateOrderItemDto(1, 0);

            dto.Quantity.Should().Be(0);
        }

        [Fact]
        public void CreateOrderItemDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var dto1 = new CreateOrderItemDto(1, 5);
            var dto2 = new CreateOrderItemDto(1, 5);

            dto1.Should().Be(dto2);
        }

        [Fact]
        public void CreateOrderItemDto_DifferentProductId_ShouldNotBeEqual()
        {
            var dto1 = new CreateOrderItemDto(1, 5);
            var dto2 = new CreateOrderItemDto(2, 5);

            dto1.Should().NotBe(dto2);
        }

        [Fact]
        public void CreateOrderItemDto_DifferentQuantity_ShouldNotBeEqual()
        {
            var dto1 = new CreateOrderItemDto(1, 5);
            var dto2 = new CreateOrderItemDto(1, 10);

            dto1.Should().NotBe(dto2);
        }
    }
}