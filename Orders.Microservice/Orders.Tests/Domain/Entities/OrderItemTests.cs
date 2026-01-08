using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Tests.Domain.Entities
{
    public class OrderItemTests
    {
        #region Property Initialization Tests

        [Fact]
        public void OrderItem_WhenCreated_ShouldInitializeWithDefaultValues()
        {
            var orderItem = new OrderItem();

            orderItem.Id.Should().Be(0);
            orderItem.OrderId.Should().Be(0);
            orderItem.ProductId.Should().Be(0);
            orderItem.ProductName.Should().Be(string.Empty);
            orderItem.Quantity.Should().Be(0);
            orderItem.UnitPrice.Should().Be(0m);
            orderItem.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            orderItem.Subtotal.Should().Be(0m);
        }

        [Fact]
        public void OrderItem_WhenCreatedWithValues_ShouldStoreCorrectly()
        {
            var createdAt = DateTime.UtcNow.AddDays(-1);

            var orderItem = new OrderItem
            {
                Id = 1,
                OrderId = 10,
                ProductId = 5,
                ProductName = "Produto Teste",
                Quantity = 3,
                UnitPrice = 25.50m,
                CreatedAt = createdAt
            };

            orderItem.Id.Should().Be(1);
            orderItem.OrderId.Should().Be(10);
            orderItem.ProductId.Should().Be(5);
            orderItem.ProductName.Should().Be("Produto Teste");
            orderItem.Quantity.Should().Be(3);
            orderItem.UnitPrice.Should().Be(25.50m);
            orderItem.CreatedAt.Should().Be(createdAt);
        }

        #endregion

        #region Subtotal Calculation Tests

        [Fact]
        public void Subtotal_WithPositiveQuantityAndPrice_CalculatesCorrectly()
        {
            var orderItem = new OrderItem
            {
                Quantity = 3,
                UnitPrice = 25.50m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(76.50m);
        }

        [Fact]
        public void Subtotal_WithSingleQuantity_ReturnsUnitPrice()
        {
            var orderItem = new OrderItem
            {
                Quantity = 1,
                UnitPrice = 99.99m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(99.99m);
        }

        [Fact]
        public void Subtotal_WithZeroQuantity_ReturnsZero()
        {
            var orderItem = new OrderItem
            {
                Quantity = 0,
                UnitPrice = 50.00m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(0m);
        }

        [Fact]
        public void Subtotal_WithZeroPrice_ReturnsZero()
        {
            var orderItem = new OrderItem
            {
                Quantity = 5,
                UnitPrice = 0m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(0m);
        }

        [Fact]
        public void Subtotal_WithLargeQuantity_CalculatesCorrectly()
        {
            var orderItem = new OrderItem
            {
                Quantity = 100,
                UnitPrice = 12.75m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(1275.00m);
        }

        [Theory]
        [InlineData(2, 25.50, 51.00)]
        [InlineData(5, 10.00, 50.00)]
        [InlineData(1, 15.75, 15.75)]
        [InlineData(10, 3.33, 33.30)]
        [InlineData(3, 99.99, 299.97)]
        public void Subtotal_WithVariousQuantitiesAndPrices_CalculatesCorrectly(
            int quantity,
            decimal unitPrice,
            decimal expectedSubtotal)
        {
            var orderItem = new OrderItem
            {
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(expectedSubtotal);
        }

        [Fact]
        public void Subtotal_WithDecimalPrices_MaintainsPrecision()
        {
            var orderItem = new OrderItem
            {
                Quantity = 3,
                UnitPrice = 12.345m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(37.035m);
        }

        #endregion

        #region Property Update Tests

        [Fact]
        public void Subtotal_WhenQuantityChanges_RecalculatesAutomatically()
        {
            var orderItem = new OrderItem
            {
                Quantity = 2,
                UnitPrice = 10.00m
            };

            orderItem.Subtotal.Should().Be(20.00m);
            orderItem.Quantity = 5;
            orderItem.Subtotal.Should().Be(50.00m);
        }

        [Fact]
        public void Subtotal_WhenUnitPriceChanges_RecalculatesAutomatically()
        {
            var orderItem = new OrderItem
            {
                Quantity = 3,
                UnitPrice = 10.00m
            };

            orderItem.Subtotal.Should().Be(30.00m);

            orderItem.UnitPrice = 15.00m;

            orderItem.Subtotal.Should().Be(45.00m);
        }

        [Fact]
        public void Subtotal_WhenBothQuantityAndPriceChange_RecalculatesCorrectly()
        {
            var orderItem = new OrderItem
            {
                Quantity = 2,
                UnitPrice = 10.00m
            };

            orderItem.Subtotal.Should().Be(20.00m);

            orderItem.Quantity = 4;
            orderItem.UnitPrice = 12.50m;
            orderItem.Subtotal.Should().Be(50.00m);
        }

        #endregion

        #region Navigation Property Tests

        [Fact]
        public void Order_NavigationProperty_CanBeSet()
        {
            var order = new Order
            {
                Id = 1,
                Number = 100,
                Status = OrderStatusEnum.RECEIVED
            };

            var orderItem = new OrderItem
            {
                OrderId = 1,
                ProductId = 5,
                Quantity = 2,
                UnitPrice = 25.00m
            };

            orderItem.Order = order;

            orderItem.Order.Should().NotBeNull();
            orderItem.Order.Id.Should().Be(1);
            orderItem.Order.Number.Should().Be(100);
            orderItem.OrderId.Should().Be(orderItem.Order.Id);
        }

        #endregion

        #region ProductName Tests

        [Fact]
        public void ProductName_WithEmptyString_ShouldBeAccepted()
        {
            var orderItem = new OrderItem
            {
                ProductName = string.Empty
            };

            orderItem.ProductName.Should().Be(string.Empty);
        }

        [Fact]
        public void ProductName_WithLongName_ShouldBeStored()
        {
            var longProductName = new string('A', 200);

            var orderItem = new OrderItem
            {
                ProductName = longProductName
            };

            orderItem.ProductName.Should().Be(longProductName);
            orderItem.ProductName.Length.Should().Be(200);
        }

        [Fact]
        public void ProductName_WithSpecialCharacters_ShouldBeStored()
        {
            var specialName = "Produto @#$% & Test! 123";

            var orderItem = new OrderItem
            {
                ProductName = specialName
            };

            orderItem.ProductName.Should().Be(specialName);
        }

        #endregion

        #region CreatedAt Tests

        [Fact]
        public void CreatedAt_WhenNotSet_ShouldDefaultToCurrentTime()
        {
            var beforeCreation = DateTime.UtcNow;

            var orderItem = new OrderItem();

            var afterCreation = DateTime.UtcNow;
            orderItem.CreatedAt.Should().BeOnOrAfter(beforeCreation);
            orderItem.CreatedAt.Should().BeOnOrBefore(afterCreation);
        }

        [Fact]
        public void CreatedAt_CanBeSetToSpecificDate()
        {
            var specificDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            var orderItem = new OrderItem
            {
                CreatedAt = specificDate
            };

            orderItem.CreatedAt.Should().Be(specificDate);
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Fact]
        public void OrderItem_WithNegativeQuantity_CalculatesNegativeSubtotal()
        {
            var orderItem = new OrderItem
            {
                Quantity = -2,
                UnitPrice = 10.00m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(-20.00m);
        }

        [Fact]
        public void OrderItem_WithNegativePrice_CalculatesNegativeSubtotal()
        {
            var orderItem = new OrderItem
            {
                Quantity = 3,
                UnitPrice = -5.00m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(-15.00m);
        }

        [Fact]
        public void OrderItem_WithVerySmallPrice_MaintainsPrecision()
        {
            var orderItem = new OrderItem
            {
                Quantity = 1000,
                UnitPrice = 0.01m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(10.00m);
        }

        [Fact]
        public void OrderItem_WithMaxDecimalPrice_DoesNotOverflow()
        {
            var orderItem = new OrderItem
            {
                Quantity = 1,
                UnitPrice = 999999.99m
            };

            var subtotal = orderItem.Subtotal;

            subtotal.Should().Be(999999.99m);
        }

        #endregion

        #region Complete Scenario Tests

        [Fact]
        public void OrderItem_CompleteScenario_AllPropertiesWorkTogether()
        {
            var order = new Order
            {
                Id = 100,
                Number = 1001,
                Status = OrderStatusEnum.RECEIVED
            };

            var createdDate = new DateTime(2024, 1, 10, 14, 30, 0, DateTimeKind.Utc);

            var orderItem = new OrderItem
            {
                Id = 1,
                OrderId = 100,
                ProductId = 42,
                ProductName = "Hambúrguer Especial",
                Quantity = 2,
                UnitPrice = 28.90m,
                CreatedAt = createdDate,
                Order = order
            };

            orderItem.Id.Should().Be(1);
            orderItem.OrderId.Should().Be(100);
            orderItem.ProductId.Should().Be(42);
            orderItem.ProductName.Should().Be("Hambúrguer Especial");
            orderItem.Quantity.Should().Be(2);
            orderItem.UnitPrice.Should().Be(28.90m);
            orderItem.Subtotal.Should().Be(57.80m);
            orderItem.CreatedAt.Should().Be(createdDate);
            orderItem.Order.Should().NotBeNull();
            orderItem.Order.Id.Should().Be(100);
        }

        [Fact]
        public void OrderItem_MultipleItems_CanExistIndependently()
        {
            var item1 = new OrderItem
            {
                Id = 1,
                ProductId = 10,
                ProductName = "Produto 1",
                Quantity = 2,
                UnitPrice = 10.00m
            };

            var item2 = new OrderItem
            {
                Id = 2,
                ProductId = 20,
                ProductName = "Produto 2",
                Quantity = 3,
                UnitPrice = 15.00m
            };

            item1.Subtotal.Should().Be(20.00m);
            item2.Subtotal.Should().Be(45.00m);
            item1.ProductId.Should().NotBe(item2.ProductId);
            item1.Subtotal.Should().NotBe(item2.Subtotal);
        }

        #endregion
    }
}