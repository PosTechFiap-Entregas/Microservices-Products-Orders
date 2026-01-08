using FluentAssertions;
using Orders.Application.DTOs;

namespace Orders.Tests.Application.DTOs
{
    public class OrderItemDtoTests
    {
        [Fact]
        public void OrderItemDto_WithValidData_ShouldCreateSuccessfully()
        {
            var id = 1;
            var productId = 10;
            var productName = "Produto Teste";
            var quantity = 3;
            var unitPrice = 25.50m;
            var subtotal = 76.50m;

            var dto = new OrderItemDto(id, productId, productName, quantity, unitPrice, subtotal);

            dto.Should().NotBeNull();
            dto.Id.Should().Be(id);
            dto.ProductId.Should().Be(productId);
            dto.ProductName.Should().Be(productName);
            dto.Quantity.Should().Be(quantity);
            dto.UnitPrice.Should().Be(unitPrice);
            dto.Subtotal.Should().Be(subtotal);
        }

        [Fact]
        public void OrderItemDto_WithZeroValues_ShouldCreateSuccessfully()
        {
            var dto = new OrderItemDto(1, 1, "Produto", 0, 0m, 0m);

            dto.Quantity.Should().Be(0);
            dto.UnitPrice.Should().Be(0m);
            dto.Subtotal.Should().Be(0m);
        }

        [Fact]
        public void OrderItemDto_SubtotalCalculation_ShouldBeCorrect()
        {
            var quantity = 4;
            var unitPrice = 12.75m;
            var expectedSubtotal = 51.00m;

            var dto = new OrderItemDto(1, 1, "Produto", quantity, unitPrice, expectedSubtotal);

            dto.Subtotal.Should().Be(expectedSubtotal);
            dto.Subtotal.Should().Be(quantity * unitPrice);
        }

        [Fact]
        public void OrderItemDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var dto1 = new OrderItemDto(1, 10, "Produto", 2, 25.00m, 50.00m);
            var dto2 = new OrderItemDto(1, 10, "Produto", 2, 25.00m, 50.00m);

            dto1.Should().Be(dto2);
        }
    }
}