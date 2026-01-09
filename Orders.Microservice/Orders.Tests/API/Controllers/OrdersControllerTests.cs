using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.API.Controllers;
using Orders.Application.DTOs;
using Orders.Application.Services.Interface;
using Orders.Domain.Enums;

namespace Orders.Tests.API.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _serviceMock;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _serviceMock = new Mock<IOrderService>();
            _loggerMock = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_serviceMock.Object, _loggerMock.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithOrders_ReturnsOkWithAllOrders()
        {
            var orders = new List<OrderDto>
            {
                CreateSampleOrderDto(1, 100),
                CreateSampleOrderDto(2, 101),
                CreateSampleOrderDto(3, 102)
            };

            _serviceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(orders);

            var result = await _controller.GetAll();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
            returnedOrders.Should().HaveCount(3);
            returnedOrders.Should().BeEquivalentTo(orders);

            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WithNoOrders_ReturnsOkWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<OrderDto>());

            var result = await _controller.GetAll();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
            returnedOrders.Should().BeEmpty();

            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithExistingOrder_ReturnsOkWithOrder()
        {
            var order = CreateSampleOrderDto(1, 100);

            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(order);

            var result = await _controller.GetById(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrder = okResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnedOrder.Should().BeEquivalentTo(order);

            _serviceMock.Verify(s => s.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetById_WithNonExistingOrder_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((OrderDto?)null);

            var result = await _controller.GetById(999);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Pedido com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.GetByIdAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task GetById_WithDifferentValidIds_ReturnsOk(int id)
        {
            var order = CreateSampleOrderDto(id, 100 + id);

            _serviceMock.Setup(s => s.GetByIdAsync(id))
                .ReturnsAsync(order);

            var result = await _controller.GetById(id);

            result.Result.Should().BeOfType<OkObjectResult>();
            _serviceMock.Verify(s => s.GetByIdAsync(id), Times.Once);
        }

        #endregion

        #region GetActive Tests

        [Fact]
        public async Task GetActive_WithActiveOrders_ReturnsOkWithActiveOrders()
        {
            var activeOrders = new List<OrderDto>
            {
                CreateSampleOrderDto(1, 100, OrderStatusEnum.RECEIVED),
                CreateSampleOrderDto(2, 101, OrderStatusEnum.IN_PREPARATION)
            };

            _serviceMock.Setup(s => s.GetActiveOrdersAsync())
                .ReturnsAsync(activeOrders);

            var result = await _controller.GetActive();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
            returnedOrders.Should().HaveCount(2);
            returnedOrders.All(o => o.Status != OrderStatusEnum.FINALIZED).Should().BeTrue();

            _serviceMock.Verify(s => s.GetActiveOrdersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActive_WithNoActiveOrders_ReturnsOkWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetActiveOrdersAsync())
                .ReturnsAsync(new List<OrderDto>());

            var result = await _controller.GetActive();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
            returnedOrders.Should().BeEmpty();

            _serviceMock.Verify(s => s.GetActiveOrdersAsync(), Times.Once);
        }

        #endregion

        #region GetByStatus Tests

        [Theory]
        [InlineData(OrderStatusEnum.RECEIVED)]
        [InlineData(OrderStatusEnum.IN_PREPARATION)]
        [InlineData(OrderStatusEnum.READY)]
        [InlineData(OrderStatusEnum.FINALIZED)]
        public async Task GetByStatus_WithSpecificStatus_ReturnsOkWithFilteredOrders(OrderStatusEnum status)
        {
            var orders = new List<OrderDto>
            {
                CreateSampleOrderDto(1, 100, status),
                CreateSampleOrderDto(2, 101, status)
            };

            _serviceMock.Setup(s => s.GetByStatusAsync(status))
                .ReturnsAsync(orders);

            var result = await _controller.GetByStatus(status);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
            returnedOrders.Should().HaveCount(2);
            returnedOrders.All(o => o.Status == status).Should().BeTrue();

            _serviceMock.Verify(s => s.GetByStatusAsync(status), Times.Once);
        }

        [Fact]
        public async Task GetByStatus_WithNoOrdersInStatus_ReturnsOkWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetByStatusAsync(OrderStatusEnum.RECEIVED))
                .ReturnsAsync(new List<OrderDto>());

            var result = await _controller.GetByStatus(OrderStatusEnum.RECEIVED);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
            returnedOrders.Should().BeEmpty();

            _serviceMock.Verify(s => s.GetByStatusAsync(OrderStatusEnum.RECEIVED), Times.Once);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedAtAction()
        {
            var createDto = new CreateOrderDto(1, "Pedido teste", new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(1, 2)
            });
            var createdOrder = CreateSampleOrderDto(1, 100);

            _serviceMock.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(createdOrder);

            var result = await _controller.Create(createDto);

            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(OrdersController.GetById));
            createdResult.RouteValues.Should().ContainKey("id");
            createdResult.RouteValues!["id"].Should().Be(1);

            var returnedOrder = createdResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnedOrder.Should().BeEquivalentTo(createdOrder);

            _serviceMock.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_WithNonExistentProduct_ReturnsBadRequest()
        {
            var createDto = new CreateOrderDto(1, "Pedido teste", new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(999, 1)
            });

            _serviceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new KeyNotFoundException("Produto com ID 999 não encontrado"));

            var result = await _controller.Create(createDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Produto com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_ReturnsBadRequest()
        {
            var createDto = new CreateOrderDto(1, "Pedido", new List<CreateOrderItemDto>());

            _serviceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new ArgumentException("O pedido deve conter ao menos um item"));

            var result = await _controller.Create(createDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "O pedido deve conter ao menos um item" });

            _serviceMock.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_LogsError()
        {
            var createDto = new CreateOrderDto(1, "Pedido", new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(1, 1)
            });
            var exception = new Exception("Erro ao criar pedido");

            _serviceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(exception);

            await _controller.Create(createDto);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao criar pedido")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_WithMultipleDifferentOrders_ReturnsCreatedForEach()
        {
            var createDto1 = new CreateOrderDto(1, "Pedido 1", new List<CreateOrderItemDto> { new(1, 1) });
            var createDto2 = new CreateOrderDto(2, "Pedido 2", new List<CreateOrderItemDto> { new(2, 2) });

            var createdOrder1 = CreateSampleOrderDto(1, 100);
            var createdOrder2 = CreateSampleOrderDto(2, 101);

            _serviceMock.Setup(s => s.CreateAsync(createDto1))
                .ReturnsAsync(createdOrder1);
            _serviceMock.Setup(s => s.CreateAsync(createDto2))
                .ReturnsAsync(createdOrder2);

            var result1 = await _controller.Create(createDto1);
            var result2 = await _controller.Create(createDto2);

            result1.Result.Should().BeOfType<CreatedAtActionResult>();
            result2.Result.Should().BeOfType<CreatedAtActionResult>();

            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<CreateOrderDto>()), Times.Exactly(2));
        }

        #endregion

        #region UpdateStatus Tests

        [Fact]
        public async Task UpdateStatus_WithValidStatus_ReturnsOkWithUpdatedOrder()
        {
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.IN_PREPARATION);
            var updatedOrder = CreateSampleOrderDto(1, 100, OrderStatusEnum.IN_PREPARATION);

            _serviceMock.Setup(s => s.UpdateStatusAsync(1, updateDto))
                .ReturnsAsync(updatedOrder);

            var result = await _controller.UpdateStatus(1, updateDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrder = okResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnedOrder.Should().BeEquivalentTo(updatedOrder);

            _serviceMock.Verify(s => s.UpdateStatusAsync(1, updateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_WithNonExistingOrder_ReturnsNotFound()
        {
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.IN_PREPARATION);

            _serviceMock.Setup(s => s.UpdateStatusAsync(999, updateDto))
                .ThrowsAsync(new KeyNotFoundException("Pedido com ID 999 não encontrado"));

            var result = await _controller.UpdateStatus(999, updateDto);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Pedido com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.UpdateStatusAsync(999, updateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_WithInvalidTransition_ReturnsBadRequest()
        {
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.FINALIZED);

            _serviceMock.Setup(s => s.UpdateStatusAsync(1, updateDto))
                .ThrowsAsync(new InvalidOperationException("Não é possível mudar de RECEIVED para FINALIZED"));

            var result = await _controller.UpdateStatus(1, updateDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Não é possível mudar de RECEIVED para FINALIZED" });

            _serviceMock.Verify(s => s.UpdateStatusAsync(1, updateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_WhenServiceThrowsException_ReturnsBadRequest()
        {
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.IN_PREPARATION);

            _serviceMock.Setup(s => s.UpdateStatusAsync(1, updateDto))
                .ThrowsAsync(new Exception("Erro inesperado"));

            var result = await _controller.UpdateStatus(1, updateDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Erro inesperado" });

            _serviceMock.Verify(s => s.UpdateStatusAsync(1, updateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_WhenServiceThrowsException_LogsError()
        {
            var updateDto = new UpdateOrderStatusDto(OrderStatusEnum.IN_PREPARATION);
            var exception = new Exception("Erro ao atualizar");

            _serviceMock.Setup(s => s.UpdateStatusAsync(1, updateDto))
                .ThrowsAsync(exception);

            await _controller.UpdateStatus(1, updateDto);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao atualizar status do pedido")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(OrderStatusEnum.IN_PREPARATION)]
        [InlineData(OrderStatusEnum.READY)]
        [InlineData(OrderStatusEnum.FINALIZED)]
        public async Task UpdateStatus_WithValidTransitions_ReturnsOk(OrderStatusEnum to)
        {
            var updateDto = new UpdateOrderStatusDto(to);
            var updatedOrder = CreateSampleOrderDto(1, 100, to);

            _serviceMock.Setup(s => s.UpdateStatusAsync(1, updateDto))
                .ReturnsAsync(updatedOrder);

            var result = await _controller.UpdateStatus(1, updateDto);

            result.Result.Should().BeOfType<OkObjectResult>();
            _serviceMock.Verify(s => s.UpdateStatusAsync(1, updateDto), Times.Once);
        }

        #endregion

        #region SetPaymentId Tests

        [Fact]
        public async Task SetPaymentId_WithValidPaymentId_ReturnsOkWithUpdatedOrder()
        {
            var setPaymentDto = new SetPaymentIdDto("pay_123456");
            var updatedOrder = CreateSampleOrderDto(1, 100);

            _serviceMock.Setup(s => s.SetPaymentIdAsync(1, setPaymentDto))
                .ReturnsAsync(updatedOrder);

            var result = await _controller.SetPaymentId(1, setPaymentDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrder = okResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnedOrder.Should().BeEquivalentTo(updatedOrder);

            _serviceMock.Verify(s => s.SetPaymentIdAsync(1, setPaymentDto), Times.Once);
        }

        [Fact]
        public async Task SetPaymentId_WithNonExistingOrder_ReturnsNotFound()
        {
            var setPaymentDto = new SetPaymentIdDto("pay_123456");

            _serviceMock.Setup(s => s.SetPaymentIdAsync(999, setPaymentDto))
                .ThrowsAsync(new KeyNotFoundException("Pedido com ID 999 não encontrado"));

            var result = await _controller.SetPaymentId(999, setPaymentDto);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Pedido com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.SetPaymentIdAsync(999, setPaymentDto), Times.Once);
        }

        [Fact]
        public async Task SetPaymentId_WhenServiceThrowsException_ReturnsBadRequest()
        {
            var setPaymentDto = new SetPaymentIdDto("");

            _serviceMock.Setup(s => s.SetPaymentIdAsync(1, setPaymentDto))
                .ThrowsAsync(new ArgumentException("PaymentId não pode ser vazio"));

            var result = await _controller.SetPaymentId(1, setPaymentDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "PaymentId não pode ser vazio" });

            _serviceMock.Verify(s => s.SetPaymentIdAsync(1, setPaymentDto), Times.Once);
        }

        [Fact]
        public async Task SetPaymentId_WhenServiceThrowsException_LogsError()
        {
            var setPaymentDto = new SetPaymentIdDto("pay_123");
            var exception = new Exception("Erro ao definir paymentId");

            _serviceMock.Setup(s => s.SetPaymentIdAsync(1, setPaymentDto))
                .ThrowsAsync(exception);

            await _controller.SetPaymentId(1, setPaymentDto);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao definir paymentId do pedido")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("pay_123")]
        [InlineData("pay_456")]
        [InlineData("pay_789")]
        public async Task SetPaymentId_WithDifferentPaymentIds_ReturnsOk(string paymentId)
        {
            var setPaymentDto = new SetPaymentIdDto(paymentId);
            var updatedOrder = CreateSampleOrderDto(1, 100);

            _serviceMock.Setup(s => s.SetPaymentIdAsync(1, setPaymentDto))
                .ReturnsAsync(updatedOrder);

            var result = await _controller.SetPaymentId(1, setPaymentDto);

            result.Result.Should().BeOfType<OkObjectResult>();
            _serviceMock.Verify(s => s.SetPaymentIdAsync(1, setPaymentDto), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithExistingOrder_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                .ReturnsAsync(true);

            var result = await _controller.Delete(1);

            result.Should().BeOfType<NoContentResult>();

            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Delete_WithNonExistingOrder_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(999))
                .ReturnsAsync(false);

            var result = await _controller.Delete(999);

            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Pedido com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.DeleteAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Delete_WithDifferentValidIds_ReturnsNoContent(int id)
        {
            _serviceMock.Setup(s => s.DeleteAsync(id))
                .ReturnsAsync(true);

            var result = await _controller.Delete(id);

            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task Delete_CalledMultipleTimes_InvokesServiceEachTime()
        {
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            await _controller.Delete(1);
            await _controller.Delete(2);
            await _controller.Delete(3);

            _serviceMock.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Exactly(3));
        }

        #endregion

        #region Helper Methods

        private static OrderDto CreateSampleOrderDto(
            int id = 1,
            int number = 100,
            OrderStatusEnum status = OrderStatusEnum.RECEIVED)
        {
            return new OrderDto(
                Id: id,
                CustomerId: 1,
                Status: status,
                Observation: "Pedido teste",
                Number: number,
                PaymentId: null,
                PaymentStatus: PaymentStatusEnum.PENDING,
                Total: 50.00m,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow,
                Items: new List<OrderItemDto>
                {
                    new OrderItemDto(1, 1, "Produto Teste", 2, 25.00m, 50.00m)
                }
            );
        }

        #endregion
    }
}