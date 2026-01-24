using FluentAssertions;
using Moq;
using Orders.Application.DTOs;
using Orders.Application.Services.Service;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Interfaces.Repository;
using Orders.Infrastructure.HttpClients;

namespace Orders.Tests.Application.Services.Service
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _repositoryMock;
        private readonly Mock<IProductsHttpClient> _productsClientMock;
        private readonly Mock<IPaymentHttpClient> _paymentClientMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _repositoryMock = new Mock<IOrderRepository>();
            _productsClientMock = new Mock<IProductsHttpClient>();
            _paymentClientMock = new Mock<IPaymentHttpClient>();
            _service = new OrderService(
                _repositoryMock.Object,
                _productsClientMock.Object,
                _paymentClientMock.Object);
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingOrder_ReturnsOrderDto()
        {
            var orderId = Guid.NewGuid();
            var order = CreateSampleOrder(orderId);
            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync(order);

            var result = await _service.GetByIdAsync(orderId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(order.Id);
            result.Number.Should().Be(order.Number);
            result.Status.Should().Be(order.Status);
            result.Total.Should().Be(order.TotalAmount);
            result.Items.Should().HaveCount(order.Items.Count);

            _repositoryMock.Verify(r => r.GetByIdWithItemsAsync(orderId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingOrder_ReturnsNull()
        {
            var orderId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync((Order?)null);

            var result = await _service.GetByIdAsync(orderId);

            result.Should().BeNull();
            _repositoryMock.Verify(r => r.GetByIdWithItemsAsync(orderId), Times.Once);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithOrders_ReturnsAllOrders()
        {
            var order1Id = Guid.NewGuid();
            var order2Id = Guid.NewGuid();
            var order3Id = Guid.NewGuid();

            var orders = new List<Order>
            {
                CreateSampleOrder(order1Id, 100),
                CreateSampleOrder(order2Id, 101),
                CreateSampleOrder(order3Id, 102)
            };

            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(orders);

            var result = await _service.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Select(o => o.Id).Should().BeEquivalentTo(new[] { order1Id, order2Id, order3Id });

            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WithNoOrders_ReturnsEmptyList()
        {
            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Order>());

            var result = await _service.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        #endregion

        #region GetActiveOrdersAsync Tests

        [Fact]
        public async Task GetActiveOrdersAsync_ReturnsActiveOrders()
        {
            var activeOrders = new List<Order>
            {
                CreateSampleOrder(Guid.NewGuid(), 100, OrderStatusEnum.RECEIVED),
                CreateSampleOrder(Guid.NewGuid(), 101, OrderStatusEnum.IN_PREPARATION)
            };

            _repositoryMock.Setup(r => r.GetActiveOrdersAsync())
                .ReturnsAsync(activeOrders);

            var result = await _service.GetActiveOrdersAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(o => o.Status != OrderStatusEnum.FINALIZED).Should().BeTrue();

            _repositoryMock.Verify(r => r.GetActiveOrdersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActiveOrdersAsync_WithNoActiveOrders_ReturnsEmptyList()
        {
            _repositoryMock.Setup(r => r.GetActiveOrdersAsync())
                .ReturnsAsync(new List<Order>());

            var result = await _service.GetActiveOrdersAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _repositoryMock.Verify(r => r.GetActiveOrdersAsync(), Times.Once);
        }

        #endregion

        #region GetByStatusAsync Tests

        [Theory]
        [InlineData(OrderStatusEnum.RECEIVED)]
        [InlineData(OrderStatusEnum.IN_PREPARATION)]
        [InlineData(OrderStatusEnum.READY)]
        [InlineData(OrderStatusEnum.FINALIZED)]
        public async Task GetByStatusAsync_WithSpecificStatus_ReturnsFilteredOrders(OrderStatusEnum status)
        {
            var orders = new List<Order>
            {
                CreateSampleOrder(Guid.NewGuid(), 100, status),
                CreateSampleOrder(Guid.NewGuid(), 101, status)
            };

            _repositoryMock.Setup(r => r.GetByStatusAsync(status))
                .ReturnsAsync(orders);

            var result = await _service.GetByStatusAsync(status);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(o => o.Status == status).Should().BeTrue();

            _repositoryMock.Verify(r => r.GetByStatusAsync(status), Times.Once);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidData_CreatesOrderSuccessfully()
        {
            var product = new ProductResponse(1, "Produto Teste", 25.50m, "Categoria", "Descrição", true, null);
            var createdOrderId = Guid.NewGuid();
            var paymentResponse = new PaymentResponse("pay_123", createdOrderId.ToString(), 51.00m, "PENDING", "qrcode_data", DateTime.UtcNow);

            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: "Pedido teste",
                Items: new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto(1, 2)
                }
            );

            var createdOrder = CreateSampleOrder(createdOrderId);
            createdOrder.Items.First().UnitPrice = 25.50m;
            createdOrder.RecalculateTotal();

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync())
                .ReturnsAsync(100);

            _productsClientMock.Setup(p => p.GetProductByIdAsync(1))
                .ReturnsAsync(product);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            _paymentClientMock.Setup(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(paymentResponse);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => o);

            var result = await _service.CreateAsync(createDto);

            result.Should().NotBeNull();
            result.Number.Should().Be(100);
            result.Status.Should().Be(OrderStatusEnum.RECEIVED);
            result.PaymentStatus.Should().Be(PaymentStatusEnum.PENDING);
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductId.Should().Be(1);
            result.Items.First().Quantity.Should().Be(2);
            result.Items.First().UnitPrice.Should().Be(25.50m);
            result.Total.Should().Be(51.00m);

            _repositoryMock.Verify(r => r.GetNextOrderNumberAsync(), Times.Once);
            _productsClientMock.Verify(p => p.GetProductByIdAsync(1), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            _paymentClientMock.Verify(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithMultipleItems_CalculatesTotalCorrectly()
        {
            var product1 = new ProductResponse(1, "Produto 1", 10.00m, "Categoria", null, true, null);
            var product2 = new ProductResponse(2, "Produto 2", 20.00m, "Categoria", null, true, null);
            var createdOrderId = Guid.NewGuid();
            var paymentResponse = new PaymentResponse("pay_123", createdOrderId.ToString(), 70.00m, "PENDING", "qrcode_data", DateTime.UtcNow);

            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: null,
                Items: new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto(1, 3),
                    new CreateOrderItemDto(2, 2)
                }
            );

            var createdOrder = new Order
            {
                Id = createdOrderId,
                Number = 101,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, ProductName = "Produto 1", Quantity = 3, UnitPrice = 10.00m },
                    new OrderItem { ProductId = 2, ProductName = "Produto 2", Quantity = 2, UnitPrice = 20.00m }
                }
            };
            createdOrder.RecalculateTotal();

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync())
                .ReturnsAsync(101);

            _productsClientMock.Setup(p => p.GetProductByIdAsync(1))
                .ReturnsAsync(product1);

            _productsClientMock.Setup(p => p.GetProductByIdAsync(2))
                .ReturnsAsync(product2);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            _paymentClientMock.Setup(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(paymentResponse);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => o);

            var result = await _service.CreateAsync(createDto);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Total.Should().Be(70.00m);

            _productsClientMock.Verify(p => p.GetProductByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _paymentClientMock.Verify(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNullItems_ThrowsArgumentException()
        {
            var createDto = new CreateOrderDto(1, "Teste", null!);

            Func<Task> act = async () => await _service.CreateAsync(createDto);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("O pedido deve conter ao menos um item");

            _repositoryMock.Verify(r => r.GetNextOrderNumberAsync(), Times.Never);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithEmptyItems_ThrowsArgumentException()
        {
            var createDto = new CreateOrderDto(1, "Teste", new List<CreateOrderItemDto>());

            Func<Task> act = async () => await _service.CreateAsync(createDto);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("O pedido deve conter ao menos um item");

            _repositoryMock.Verify(r => r.GetNextOrderNumberAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistentProduct_ThrowsKeyNotFoundException()
        {
            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: "Teste",
                Items: new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto(999, 1)
                }
            );

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync())
                .ReturnsAsync(100);

            _productsClientMock.Setup(p => p.GetProductByIdAsync(999))
                .ReturnsAsync((ProductResponse?)null);

            Func<Task> act = async () => await _service.CreateAsync(createDto);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Produto com ID 999 não encontrado");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithInactiveProduct_ThrowsInvalidOperationException()
        {
            var inactiveProduct = new ProductResponse(1, "Produto Inativo", 10.00m, "Categoria", null, false, null);
            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: "Teste",
                Items: new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto(1, 1)
                }
            );

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync())
                .ReturnsAsync(100);

            _productsClientMock.Setup(p => p.GetProductByIdAsync(1))
                .ReturnsAsync(inactiveProduct);

            Func<Task> act = async () => await _service.CreateAsync(createDto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Produto Produto Inativo não está ativo");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithNullCustomerId_CreatesOrderSuccessfully()
        {
            var product = new ProductResponse(1, "Produto", 15.00m, "Categoria", null, true, null);
            var createdOrderId = Guid.NewGuid();
            var paymentResponse = new PaymentResponse("pay_123", createdOrderId.ToString(), 15.00m, "PENDING", "qrcode_data", DateTime.UtcNow);

            var createDto = new CreateOrderDto(
                CustomerId: null,
                Observation: "Pedido sem cliente",
                Items: new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto(1, 1)
                }
            );

            var createdOrder = CreateSampleOrder(createdOrderId);
            createdOrder.CustomerId = null;

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync())
                .ReturnsAsync(100);

            _productsClientMock.Setup(p => p.GetProductByIdAsync(1))
                .ReturnsAsync(product);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            _paymentClientMock.Setup(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(paymentResponse);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => o);

            var result = await _service.CreateAsync(createDto);

            result.Should().NotBeNull();
            result.CustomerId.Should().BeNull();
            result.Number.Should().Be(100);
        }

        [Fact]
        public async Task CreateAsync_WhenPaymentApiFails_StillCreatesOrder()
        {
            var product = new ProductResponse(1, "Produto Teste", 25.50m, "Categoria", "Descrição", true, null);
            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: "Pedido teste",
                Items: new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto(1, 2)
                }
            );

            var createdOrder = CreateSampleOrder(Guid.NewGuid());

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync())
                .ReturnsAsync(100);

            _productsClientMock.Setup(p => p.GetProductByIdAsync(1))
                .ReturnsAsync(product);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            _paymentClientMock.Setup(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ThrowsAsync(new Exception("Payment API unavailable"));

            var result = await _service.CreateAsync(createDto);

            result.Should().NotBeNull();
            result.Number.Should().Be(100);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            _paymentClientMock.Verify(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenPaymentIsNull_DoesNotCallUpdateAsync_AndReturnsOrderWithoutPaymentId()
        {
            var product = new ProductResponse(1, "Produto", 10.00m, "Categoria", null, true, null);

            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: "Obs",
                Items: new List<CreateOrderItemDto> { new(1, 2) }
            );

            var createdOrder = new Order
            {
                Id = Guid.NewGuid(),
                Number = 100,
                Items = new List<OrderItem> { new OrderItem { ProductId = 1, ProductName = "Produto", Quantity = 2, UnitPrice = 10.00m } }
            };
            createdOrder.RecalculateTotal();

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync()).ReturnsAsync(100);
            _productsClientMock.Setup(p => p.GetProductByIdAsync(1)).ReturnsAsync(product);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync(createdOrder);
            _paymentClientMock.Setup(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync((PaymentResponse?)null);

            var result = await _service.CreateAsync(createDto);

            result.Should().NotBeNull();
            result.PaymentId.Should().BeNull();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
            _paymentClientMock.Verify(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenRepositoryReturnsOrder_CallsPaymentWithGuidString()
        {
            var product = new ProductResponse(1, "Produto", 5.00m, "Categoria", null, true, null);
            var createdOrderId = Guid.NewGuid();
            var paymentResponse = new PaymentResponse("pay_abc", createdOrderId.ToString(), 10.00m, "PENDING", "qrcode", DateTime.UtcNow);

            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: null,
                Items: new List<CreateOrderItemDto> { new(1, 2) }
            );

            var createdOrder = new Order
            {
                Id = createdOrderId,
                Number = 101,
                Items = new List<OrderItem> { new OrderItem { ProductId = 1, ProductName = "Produto", Quantity = 2, UnitPrice = 5.00m } }
            };
            createdOrder.RecalculateTotal();

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync()).ReturnsAsync(101);
            _productsClientMock.Setup(p => p.GetProductByIdAsync(1)).ReturnsAsync(product);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync(createdOrder);
            _paymentClientMock.Setup(p => p.CreatePaymentAsync(createdOrderId.ToString(), createdOrder.TotalAmount)).ReturnsAsync(paymentResponse);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);

            var result = await _service.CreateAsync(createDto);

            result.Should().NotBeNull();
            _paymentClientMock.Verify(p => p.CreatePaymentAsync(createdOrderId.ToString(), createdOrder.TotalAmount), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.PaymentId == "pay_abc")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenPaymentThrowsException_DoesNotCallUpdateAsync_ButCreatesOrder()
        {
            var product = new ProductResponse(1, "Produto", 7.50m, "Categoria", null, true, null);

            var createDto = new CreateOrderDto(
                CustomerId: 1,
                Observation: "Obs",
                Items: new List<CreateOrderItemDto> { new(1, 1) }
            );

            var createdOrder = new Order
            {
                Id = Guid.NewGuid(),
                Number = 102,
                Items = new List<OrderItem> { new OrderItem { ProductId = 1, ProductName = "Produto", Quantity = 1, UnitPrice = 7.50m } }
            };
            createdOrder.RecalculateTotal();

            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync()).ReturnsAsync(102);
            _productsClientMock.Setup(p => p.GetProductByIdAsync(1)).ReturnsAsync(product);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync(createdOrder);
            _paymentClientMock.Setup(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ThrowsAsync(new Exception("Payment API down"));

            var result = await _service.CreateAsync(createDto);

            result.Should().NotBeNull();
            result.Number.Should().Be(102);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
            _paymentClientMock.Verify(p => p.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        }

        #endregion

        #region UpdateStatusAsync Tests

        [Fact]
        public async Task UpdateStatusAsync_WithValidStatus_UpdatesSuccessfully()
        {
            var orderId = Guid.NewGuid();
            var order = CreateSampleOrder(orderId, 100, OrderStatusEnum.RECEIVED);
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.IN_PREPARATION);

            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync(order);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => o);

            var result = await _service.UpdateStatusAsync(orderId, updateDto);

            result.Should().NotBeNull();
            result.Status.Should().Be(OrderStatusEnum.IN_PREPARATION);

            _repositoryMock.Verify(r => r.GetByIdWithItemsAsync(orderId), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            var orderId = Guid.NewGuid();
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.IN_PREPARATION);

            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync((Order?)null);

            Func<Task> act = async () => await _service.UpdateStatusAsync(orderId, updateDto);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Pedido com ID {orderId} não encontrado");

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_WithInvalidTransition_ThrowsInvalidOperationException()
        {
            var orderId = Guid.NewGuid();
            var order = CreateSampleOrder(orderId, 100, OrderStatusEnum.RECEIVED);
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.FINALIZED);

            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync(order);

            Func<Task> act = async () => await _service.UpdateStatusAsync(orderId, updateDto);

            await act.Should().ThrowAsync<InvalidOperationException>();

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }

        #endregion

        #region SetPaymentIdAsync Tests

        [Fact]
        public async Task SetPaymentIdAsync_WithValidPaymentId_UpdatesSuccessfully()
        {
            var orderId = Guid.NewGuid();
            var order = CreateSampleOrder(orderId, 100);
            var setPaymentDto = new SetPaymentIdDto("pay_123456");

            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync(order);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => o);

            var result = await _service.SetPaymentIdAsync(orderId, setPaymentDto);

            result.Should().NotBeNull();
            result.PaymentId.Should().Be("pay_123456");

            _repositoryMock.Verify(r => r.GetByIdWithItemsAsync(orderId), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task SetPaymentIdAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
        {
            var orderId = Guid.NewGuid();
            var setPaymentDto = new SetPaymentIdDto("pay_123456");

            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync((Order?)null);

            Func<Task> act = async () => await _service.SetPaymentIdAsync(orderId, setPaymentDto);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Pedido com ID {orderId} não encontrado");

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task SetPaymentIdAsync_WithEmptyPaymentId_ThrowsArgumentException()
        {
            var orderId = Guid.NewGuid();
            var order = CreateSampleOrder(orderId, 100);
            var setPaymentDto = new SetPaymentIdDto("");

            _repositoryMock.Setup(r => r.GetByIdWithItemsAsync(orderId))
                .ReturnsAsync(order);

            Func<Task> act = async () => await _service.SetPaymentIdAsync(orderId, setPaymentDto);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("PaymentId não pode ser vazio");

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithExistingOrder_ReturnsTrue()
        {
            var orderId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.DeleteAsync(orderId))
                .ReturnsAsync(true);

            var result = await _service.DeleteAsync(orderId);

            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteAsync(orderId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentOrder_ReturnsFalse()
        {
            var orderId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.DeleteAsync(orderId))
                .ReturnsAsync(false);

            var result = await _service.DeleteAsync(orderId);

            result.Should().BeFalse();
            _repositoryMock.Verify(r => r.DeleteAsync(orderId), Times.Once);
        }

        #endregion

        #region Helper Methods

        private static Order CreateSampleOrder(
            Guid? id = null,
            int number = 100,
            OrderStatusEnum status = OrderStatusEnum.RECEIVED)
        {
            var orderId = id ?? Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                CustomerId = 1,
                Number = number,
                Status = status,
                Observation = "Pedido teste",
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        OrderId = orderId,
                        ProductId = 1,
                        ProductName = "Produto Teste",
                        Quantity = 2,
                        UnitPrice = 25.00m
                    }
                }
            };

            order.RecalculateTotal();
            return order;
        }

        #endregion
    }
}