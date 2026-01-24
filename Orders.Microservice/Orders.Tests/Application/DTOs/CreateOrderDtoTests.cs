using FluentAssertions;
using Orders.Application.DTOs;

namespace Orders.Tests.Application.DTOs
{
    public class CreateOrderDtoTests
    {
        [Fact]
        public void CreateOrderDto_WithValidData_ShouldCreateSuccessfully()
        {
            var customerId = Guid.NewGuid();
            var observation = "Pedido teste";
            var items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(1, 2),
                new CreateOrderItemDto(2, 3)
            };
            var dto = new CreateOrderDto(customerId, observation, items);
            dto.Should().NotBeNull();
            dto.CustomerId.Should().Be(customerId);
            dto.Observation.Should().Be(observation);
            dto.Items.Should().HaveCount(2);
            dto.Items.Should().BeEquivalentTo(items);
        }

        [Fact]
        public void CreateOrderDto_WithNullCustomerId_ShouldCreateSuccessfully()
        {
            var items = new List<CreateOrderItemDto> { new CreateOrderItemDto(1, 1) };
            var dto = new CreateOrderDto(null, "Observação", items);
            dto.CustomerId.Should().BeNull();
            dto.Observation.Should().Be("Observação");
            dto.Items.Should().HaveCount(1);
        }

        [Fact]
        public void CreateOrderDto_WithNullObservation_ShouldCreateSuccessfully()
        {
            var customerId = Guid.NewGuid();
            var items = new List<CreateOrderItemDto> { new CreateOrderItemDto(1, 1) };

            var dto = new CreateOrderDto(customerId, null, items);

            dto.CustomerId.Should().Be(customerId);
            dto.Observation.Should().BeNull();
            dto.Items.Should().HaveCount(1);
        }

        [Fact]
        public void CreateOrderDto_WithEmptyItems_ShouldCreateSuccessfully()
        {
            var items = new List<CreateOrderItemDto>();
            var dto = new CreateOrderDto(Guid.NewGuid(), "Teste", items);
            dto.Items.Should().BeEmpty();
        }

        [Fact]
        public void CreateOrderDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var customerId = Guid.NewGuid();
            var items = new List<CreateOrderItemDto> { new CreateOrderItemDto(1, 2) };

            var dto1 = new CreateOrderDto(customerId, "Teste", items);
            var dto2 = new CreateOrderDto(customerId, "Teste", items);

            dto1.Should().Be(dto2);
        }

        [Fact]
        public void CreateOrderDto_DifferentValues_ShouldNotBeEqual()
        {
            var items = new List<CreateOrderItemDto> { new CreateOrderItemDto(1, 2) };

            var dto1 = new CreateOrderDto(Guid.NewGuid(), "Teste", items);
            var dto2 = new CreateOrderDto(Guid.NewGuid(), "Teste", items);

            dto1.Should().NotBe(dto2);
        }
    }
}