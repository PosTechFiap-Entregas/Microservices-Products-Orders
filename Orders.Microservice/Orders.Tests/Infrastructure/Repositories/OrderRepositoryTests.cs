using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.Repositories;

namespace Orders.Tests.Infrastructure.Repositories
{
    public class OrderRepositoryTests : IDisposable
    {
        private readonly OrdersDbContext _context;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new OrdersDbContext(options);
            _repository = new OrderRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingOrder_ReturnsOrder()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Number.Should().Be(100);
            result.Status.Should().Be(OrderStatusEnum.RECEIVED);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingOrder_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldNotIncludeItems()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldUseAsNoTracking()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetByIdAsync(1);

            result.Should().NotBeNull();
            var trackedEntities = _context.ChangeTracker.Entries<Order>().ToList();
            trackedEntities.Should().BeEmpty();
        }

        #endregion

        #region GetByIdWithItemsAsync Tests

        [Fact]
        public async Task GetByIdWithItemsAsync_WithExistingOrder_ReturnsOrderWithItems()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetByIdWithItemsAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Number.Should().Be(100);
            result.Items.Should().HaveCount(2);
            result.Items.First().ProductName.Should().Be("Produto 1");
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_WithNonExistingOrder_ReturnsNull()
        {
            var result = await _repository.GetByIdWithItemsAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdWithItemsAsync_ShouldUseAsNoTracking()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetByIdWithItemsAsync(1);

            result.Should().NotBeNull();
            var trackedEntities = _context.ChangeTracker.Entries<Order>().ToList();
            trackedEntities.Should().BeEmpty();
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithMultipleOrders_ReturnsAllOrdersWithItems()
        {
            var order1 = CreateSampleOrder(1, 100);
            var order2 = CreateSampleOrder(2, 101);
            var order3 = CreateSampleOrder(3, 102);

            await _context.Orders.AddRangeAsync(order1, order2, order3);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(3);
            result.All(o => o.Items.Any()).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrderByCreatedAtDescending()
        {
            var order1 = CreateSampleOrder(1, 100);
            order1.CreatedAt = DateTime.UtcNow.AddHours(-3);

            var order2 = CreateSampleOrder(2, 101);
            order2.CreatedAt = DateTime.UtcNow.AddHours(-1);

            var order3 = CreateSampleOrder(3, 102);
            order3.CreatedAt = DateTime.UtcNow;

            await _context.Orders.AddRangeAsync(order1, order2, order3);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetAllAsync()).ToList();

            result.Should().HaveCount(3);
            result[0].Id.Should().Be(3);
            result[1].Id.Should().Be(2);
            result[2].Id.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_WithNoOrders_ReturnsEmptyList()
        {
            var result = await _repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldIncludeItems()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetAllAsync()).ToList();

            result.Should().HaveCount(1);
            result[0].Items.Should().HaveCount(2);
        }

        #endregion

        #region GetActiveOrdersAsync Tests

        [Fact]
        public async Task GetActiveOrdersAsync_ReturnsOnlyNonFinalizedOrders()
        {
            var order1 = CreateSampleOrder(1, 100, OrderStatusEnum.RECEIVED);
            var order2 = CreateSampleOrder(2, 101, OrderStatusEnum.IN_PREPARATION);
            var order3 = CreateSampleOrder(3, 102, OrderStatusEnum.READY);
            var order4 = CreateSampleOrder(4, 103, OrderStatusEnum.FINALIZED);

            await _context.Orders.AddRangeAsync(order1, order2, order3, order4);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetActiveOrdersAsync()).ToList();

            result.Should().HaveCount(3);
            result.Should().NotContain(o => o.Status == OrderStatusEnum.FINALIZED);
            result.Select(o => o.Id).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task GetActiveOrdersAsync_ShouldOrderByCreatedAtAscending()
        {
            var order1 = CreateSampleOrder(1, 100, OrderStatusEnum.RECEIVED);
            order1.CreatedAt = DateTime.UtcNow.AddHours(-3);

            var order2 = CreateSampleOrder(2, 101, OrderStatusEnum.IN_PREPARATION);
            order2.CreatedAt = DateTime.UtcNow.AddHours(-1);

            var order3 = CreateSampleOrder(3, 102, OrderStatusEnum.READY);
            order3.CreatedAt = DateTime.UtcNow;

            await _context.Orders.AddRangeAsync(order1, order2, order3);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetActiveOrdersAsync()).ToList();

            result.Should().HaveCount(3);
            result[0].Id.Should().Be(1); 
            result[1].Id.Should().Be(2);
            result[2].Id.Should().Be(3);
        }

        [Fact]
        public async Task GetActiveOrdersAsync_WithNoActiveOrders_ReturnsEmptyList()
        {
            var order = CreateSampleOrder(1, 100, OrderStatusEnum.FINALIZED);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetActiveOrdersAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActiveOrdersAsync_ShouldIncludeItems()
        {
            var order = CreateSampleOrder(1, 100, OrderStatusEnum.RECEIVED);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetActiveOrdersAsync()).ToList();

            result.Should().HaveCount(1);
            result[0].Items.Should().HaveCount(2);
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
            var order1 = CreateSampleOrder(1, 100, status);
            var order2 = CreateSampleOrder(2, 101, status);

            var otherStatus = status == OrderStatusEnum.FINALIZED ? OrderStatusEnum.RECEIVED : OrderStatusEnum.FINALIZED;
            var order3 = CreateSampleOrder(3, 102, otherStatus);

            await _context.Orders.AddRangeAsync(order1, order2, order3);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetByStatusAsync(status)).ToList();

            result.Should().HaveCount(2);
            result.All(o => o.Status == status).Should().BeTrue();
            result.Select(o => o.Id).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldOrderByCreatedAtAscending()
        {
            var order1 = CreateSampleOrder(1, 100, OrderStatusEnum.RECEIVED);
            order1.CreatedAt = DateTime.UtcNow.AddHours(-2);

            var order2 = CreateSampleOrder(2, 101, OrderStatusEnum.RECEIVED);
            order2.CreatedAt = DateTime.UtcNow;

            await _context.Orders.AddRangeAsync(order1, order2);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetByStatusAsync(OrderStatusEnum.RECEIVED)).ToList();

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[1].Id.Should().Be(2);
        }

        [Fact]
        public async Task GetByStatusAsync_WithNoMatchingStatus_ReturnsEmptyList()
        {
            var order = CreateSampleOrder(1, 100, OrderStatusEnum.RECEIVED);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetByStatusAsync(OrderStatusEnum.FINALIZED);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldIncludeItems()
        {
            var order = CreateSampleOrder(1, 100, OrderStatusEnum.RECEIVED);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = (await _repository.GetByStatusAsync(OrderStatusEnum.RECEIVED)).ToList();

            result.Should().HaveCount(1);
            result[0].Items.Should().HaveCount(2);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidOrder_SavesAndReturnsOrderWithItems()
        {
            var order = CreateSampleOrder(0, 100);

            var result = await _repository.AddAsync(order);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Number.Should().Be(100);
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddAsync_ShouldPersistToDatabase()
        {
            var order = CreateSampleOrder(0, 100);

            await _repository.AddAsync(order);

            var savedOrder = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Number == 100);

            savedOrder.Should().NotBeNull();
            savedOrder!.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddAsync_WithMultipleItems_SavesAllItems()
        {
            var order = new Order
            {
                Number = 200,
                Status = OrderStatusEnum.RECEIVED,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, ProductName = "Item 1", Quantity = 1, UnitPrice = 10.00m },
                    new OrderItem { ProductId = 2, ProductName = "Item 2", Quantity = 2, UnitPrice = 20.00m },
                    new OrderItem { ProductId = 3, ProductName = "Item 3", Quantity = 3, UnitPrice = 30.00m }
                }
            };

            var result = await _repository.AddAsync(order);

            result.Items.Should().HaveCount(3);
            result.Items.All(i => i.Id > 0).Should().BeTrue();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithExistingOrder_UpdatesAndReturnsOrder()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var orderToUpdate = await _context.Orders
                .Include(o => o.Items)
                .FirstAsync(o => o.Id == 1);

            orderToUpdate.Observation = "Observação atualizada";
            orderToUpdate.Status = OrderStatusEnum.IN_PREPARATION;

            var result = await _repository.UpdateAsync(orderToUpdate);

            result.Should().NotBeNull();
            result.Observation.Should().Be("Observação atualizada");
            result.Status.Should().Be(OrderStatusEnum.IN_PREPARATION);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUpdatedAtField()
        {
            var order = CreateSampleOrder(1, 100);
            var originalUpdatedAt = order.UpdatedAt;
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            await Task.Delay(100);

            var orderToUpdate = await _context.Orders.FirstAsync(o => o.Id == 1);

            var result = await _repository.UpdateAsync(orderToUpdate);

            result.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Fact]
        public async Task UpdateAsync_ShouldPersistChangesToDatabase()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var orderToUpdate = await _context.Orders.FirstAsync(o => o.Id == 1);
            orderToUpdate.PaymentId = "pay_updated_123";

            await _repository.UpdateAsync(orderToUpdate);
            _context.ChangeTracker.Clear();

            var updatedOrder = await _context.Orders.FindAsync(1);
            updatedOrder!.PaymentId.Should().Be("pay_updated_123");
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithExistingOrder_DeletesAndReturnsTrue()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.DeleteAsync(1);

            result.Should().BeTrue();
            var deletedOrder = await _context.Orders.FindAsync(1);
            deletedOrder.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingOrder_ReturnsFalse()
        {
            var result = await _repository.DeleteAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveFromDatabase()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            await _repository.DeleteAsync(1);

            var count = await _context.Orders.CountAsync();
            count.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCascadeDeleteItems()
        {
            var order = CreateSampleOrder(1, 100);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(1);

            var itemsCount = await _context.Set<OrderItem>().CountAsync();
            itemsCount.Should().Be(0);
        }

        #endregion

        #region GetNextOrderNumberAsync Tests

        [Fact]
        public async Task GetNextOrderNumberAsync_WithNoOrders_ReturnsOne()
        {
            var result = await _repository.GetNextOrderNumberAsync();

            result.Should().Be(1);
        }

        [Fact]
        public async Task GetNextOrderNumberAsync_WithExistingOrders_ReturnsMaxPlusOne()
        {
            var order1 = CreateSampleOrder(1, 100);
            var order2 = CreateSampleOrder(2, 150);
            var order3 = CreateSampleOrder(3, 125);

            await _context.Orders.AddRangeAsync(order1, order2, order3);
            await _context.SaveChangesAsync();

            var result = await _repository.GetNextOrderNumberAsync();

            result.Should().Be(151);
        }

        [Fact]
        public async Task GetNextOrderNumberAsync_ShouldBeSequential()
        {
            var firstNumber = await _repository.GetNextOrderNumberAsync();

            var order = CreateSampleOrder(0, firstNumber);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var secondNumber = await _repository.GetNextOrderNumberAsync();

            firstNumber.Should().Be(1);
            secondNumber.Should().Be(2);
        }

        [Fact]
        public async Task GetNextOrderNumberAsync_WithDeletedOrders_StillReturnsCorrectNumber()
        {
            var order1 = CreateSampleOrder(1, 100);
            var order2 = CreateSampleOrder(2, 101);
            await _context.Orders.AddRangeAsync(order1, order2);
            await _context.SaveChangesAsync();

            _context.Orders.Remove(order2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetNextOrderNumberAsync();

            result.Should().Be(101);
        }

        #endregion

        #region Helper Methods

        private static Order CreateSampleOrder(
            int id = 0,
            int number = 100,
            OrderStatusEnum status = OrderStatusEnum.RECEIVED)
        {
            var order = new Order
            {
                Id = id,
                CustomerId = 1,
                Number = number,
                Status = status,
                Observation = "Pedido teste",
                PaymentStatus = PaymentStatusEnum.PENDING,
                TotalAmount = 75.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        ProductName = "Produto 1",
                        Quantity = 2,
                        UnitPrice = 25.00m
                    },
                    new OrderItem
                    {
                        ProductId = 2,
                        ProductName = "Produto 2",
                        Quantity = 1,
                        UnitPrice = 25.00m
                    }
                }
            };

            return order;
        }

        #endregion
    }
}