using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Infrastructure.Data;

namespace Orders.Tests.Infrastructure.Data
{
    public class OrdersDbContextTests
    {
        private OrdersDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new OrdersDbContext(options);
        }

        #region DbContext Initialization Tests

        [Fact]
        public void OrdersDbContext_CanBeInstantiated()
        {
            using var context = CreateContext();

            context.Should().NotBeNull();
        }

        [Fact]
        public void OrdersDbContext_HasOrdersDbSet()
        {
            using var context = CreateContext();

            context.Orders.Should().NotBeNull();
        }

        [Fact]
        public void OrdersDbContext_HasOrderItemsDbSet()
        {
 
            using var context = CreateContext();

            context.OrderItems.Should().NotBeNull();
        }

        #endregion

        #region Order Entity Configuration Tests

        [Fact]
        public void OrdersDbContext_OrderEntity_HasCorrectTableName()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var tableName = entityType!.GetTableName();

            tableName.Should().Be("orders");
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_HasPrimaryKey()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var primaryKey = entityType!.FindPrimaryKey();

            primaryKey.Should().NotBeNull();
            primaryKey!.Properties.Should().HaveCount(1);
            primaryKey.Properties.First().Name.Should().Be("Id");
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_IdProperty_IsConfigured()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var idProperty = entityType!.FindProperty("Id");

            idProperty.Should().NotBeNull();
            idProperty!.GetColumnName().Should().Be("id");
            idProperty.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd);
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_StatusProperty_IsConfigured()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var statusProperty = entityType!.FindProperty("Status");

            statusProperty.Should().NotBeNull();
            statusProperty!.GetColumnName().Should().Be("status");
            statusProperty.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_NumberProperty_IsConfigured()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var numberProperty = entityType!.FindProperty("Number");

            numberProperty.Should().NotBeNull();
            numberProperty!.GetColumnName().Should().Be("number");
            numberProperty.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_ObservationProperty_HasMaxLength()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var observationProperty = entityType!.FindProperty("Observation");

            observationProperty.Should().NotBeNull();
            observationProperty!.GetColumnName().Should().Be("observation");
            observationProperty.GetMaxLength().Should().Be(500);
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_PaymentIdProperty_HasMaxLength()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var paymentIdProperty = entityType!.FindProperty("PaymentId");

            paymentIdProperty.Should().NotBeNull();
            paymentIdProperty!.GetColumnName().Should().Be("payment_id");
            paymentIdProperty.GetMaxLength().Should().Be(100);
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_PaymentStatusProperty_HasDefaultValue()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var paymentStatusProperty = entityType!.FindProperty("PaymentStatus");

            paymentStatusProperty.Should().NotBeNull();
            paymentStatusProperty!.GetColumnName().Should().Be("payment_status");
            paymentStatusProperty.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_CreatedAtProperty_IsConfigured()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var createdAtProperty = entityType!.FindProperty("CreatedAt");

            createdAtProperty.Should().NotBeNull();
            createdAtProperty!.GetColumnName().Should().Be("created_at");
            createdAtProperty.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_UpdatedAtProperty_IsConfigured()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var updatedAtProperty = entityType!.FindProperty("UpdatedAt");

            updatedAtProperty.Should().NotBeNull();
            updatedAtProperty!.GetColumnName().Should().Be("updated_at");
            updatedAtProperty.IsNullable.Should().BeFalse();
        }

        #endregion

        #region OrderItem Entity Configuration Tests

        [Fact]
        public void OrdersDbContext_OrderItemEntity_HasCorrectTableName()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(OrderItem));
            var tableName = entityType!.GetTableName();

            tableName.Should().Be("order_items");
        }

        [Fact]
        public void OrdersDbContext_OrderItemEntity_HasPrimaryKey()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(OrderItem));
            var primaryKey = entityType!.FindPrimaryKey();

            primaryKey.Should().NotBeNull();
            primaryKey!.Properties.Should().HaveCount(1);
            primaryKey.Properties.First().Name.Should().Be("Id");
        }

        [Fact]
        public void OrdersDbContext_OrderItemEntity_ProductNameProperty_HasMaxLength()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(OrderItem));
            var productNameProperty = entityType!.FindProperty("ProductName");

            productNameProperty.Should().NotBeNull();
            productNameProperty!.GetColumnName().Should().Be("product_name");
            productNameProperty.GetMaxLength().Should().Be(200);
            productNameProperty.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void OrdersDbContext_OrderItemEntity_QuantityProperty_IsRequired()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(OrderItem));
            var quantityProperty = entityType!.FindProperty("Quantity");

            quantityProperty.Should().NotBeNull();
            quantityProperty!.GetColumnName().Should().Be("quantity");
            quantityProperty.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void OrdersDbContext_OrderItemEntity_UnitPriceProperty_HasCorrectType()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(OrderItem));
            var unitPriceProperty = entityType!.FindProperty("UnitPrice");

            unitPriceProperty.Should().NotBeNull();
            unitPriceProperty!.GetColumnName().Should().Be("unit_price");
            unitPriceProperty.IsNullable.Should().BeFalse();
        }

        #endregion

        #region Relationship Tests

        [Fact]
        public void OrdersDbContext_OrderToOrderItems_HasOneToManyRelationship()
        {
            using var context = CreateContext();
            var orderEntityType = context.Model.FindEntityType(typeof(Order));
            var navigation = orderEntityType!.FindNavigation("Items");

            navigation.Should().NotBeNull();
            navigation!.IsCollection.Should().BeTrue();
            navigation.ForeignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        }

        [Fact]
        public void OrdersDbContext_OrderItemToOrder_HasManyToOneRelationship()
        {
            using var context = CreateContext();
            var orderItemEntityType = context.Model.FindEntityType(typeof(OrderItem));
            var navigation = orderItemEntityType!.FindNavigation("Order");

            navigation.Should().NotBeNull();
            navigation!.IsCollection.Should().BeFalse();
        }

        [Fact]
        public void OrdersDbContext_OrderItemEntity_HasForeignKeyToOrder()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(OrderItem));
            var foreignKeys = entityType!.GetForeignKeys();
            var orderForeignKey = foreignKeys.FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Order));

            orderForeignKey.Should().NotBeNull();
            orderForeignKey!.Properties.Should().HaveCount(1);
            orderForeignKey.Properties.First().Name.Should().Be("OrderId");
        }

        [Fact]
        public void OrdersDbContext_CascadeDelete_IsConfigured()
        {
            using var context = CreateContext();
            var orderItemEntityType = context.Model.FindEntityType(typeof(OrderItem));
            var foreignKey = orderItemEntityType!.GetForeignKeys()
                .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Order));

            foreignKey.Should().NotBeNull();
            foreignKey!.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        }

        #endregion

        #region Index Tests

        [Fact]
        public void OrdersDbContext_OrderEntity_HasCustomerIdIndex()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var indexes = entityType!.GetIndexes();
            var customerIdIndex = indexes.FirstOrDefault(i =>
                i.Properties.Any(p => p.Name == "CustomerId"));

            customerIdIndex.Should().NotBeNull();
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_HasStatusIndex()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var indexes = entityType!.GetIndexes();
            var statusIndex = indexes.FirstOrDefault(i =>
                i.Properties.Any(p => p.Name == "Status"));

            statusIndex.Should().NotBeNull();
        }

        [Fact]
        public void OrdersDbContext_OrderEntity_HasUniqueNumberIndex()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(Order));
            var indexes = entityType!.GetIndexes();
            var numberIndex = indexes.FirstOrDefault(i =>
                i.Properties.Any(p => p.Name == "Number"));

            numberIndex.Should().NotBeNull();
            numberIndex!.IsUnique.Should().BeTrue();
        }

        [Fact]
        public void OrdersDbContext_OrderItemEntity_HasProductIdIndex()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(OrderItem));
            var indexes = entityType!.GetIndexes();
            var productIdIndex = indexes.FirstOrDefault(i =>
                i.Properties.Any(p => p.Name == "ProductId"));

            productIdIndex.Should().NotBeNull();
        }

        #endregion

        #region CRUD Operations Tests

        [Fact]
        public async Task OrdersDbContext_CanAddOrder()
        {
            using var context = CreateContext();
            var order = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 100,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var savedOrder = await context.Orders.FirstOrDefaultAsync(o => o.Number == 100);
            savedOrder.Should().NotBeNull();
            savedOrder!.Number.Should().Be(100);
        }

        [Fact]
        public async Task OrdersDbContext_CanAddOrderWithItems()
        {
            using var context = CreateContext();
            var order = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 101,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        ProductName = "Produto 1",
                        Quantity = 2,
                        UnitPrice = 10.00m,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var savedOrder = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Number == 101);

            savedOrder.Should().NotBeNull();
            savedOrder!.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task OrdersDbContext_CanUpdateOrder()
        {
            using var context = CreateContext();
            var order = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 102,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            order.Status = OrderStatusEnum.IN_PREPARATION;
            order.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var updatedOrder = await context.Orders.FindAsync(order.Id);
            updatedOrder!.Status.Should().Be(OrderStatusEnum.IN_PREPARATION);
        }

        [Fact]
        public async Task OrdersDbContext_CanDeleteOrder()
        {
            using var context = CreateContext();
            var order = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 103,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            var deletedOrder = await context.Orders.FindAsync(order.Id);
            deletedOrder.Should().BeNull();
        }

        [Fact]
        public async Task OrdersDbContext_CascadeDeleteWorks()
        {
            using var context = CreateContext();
            var order = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 104,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        ProductName = "Produto 1",
                        Quantity = 1,
                        UnitPrice = 10.00m,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            var orderId = order.Id;

            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            var orderItems = await context.OrderItems.Where(i => i.OrderId == orderId).ToListAsync();
            orderItems.Should().BeEmpty();
        }

        #endregion

        #region Query Tests

        [Fact]
        public async Task OrdersDbContext_CanQueryOrders()
        {
            using var context = CreateContext();
            var order1 = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 105,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var order2 = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 106,
                Status = OrderStatusEnum.IN_PREPARATION,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Orders.AddRange(order1, order2);
            await context.SaveChangesAsync();

            var orders = await context.Orders.ToListAsync();

            orders.Should().HaveCount(2);
        }

        [Fact]
        public async Task OrdersDbContext_CanFilterOrdersByStatus()
        {
            using var context = CreateContext();
            var order1 = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 107,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var order2 = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 108,
                Status = OrderStatusEnum.FINALIZED,
                PaymentStatus = PaymentStatusEnum.PAID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Orders.AddRange(order1, order2);
            await context.SaveChangesAsync();

            var receivedOrders = await context.Orders
                .Where(o => o.Status == OrderStatusEnum.RECEIVED)
                .ToListAsync();

            receivedOrders.Should().HaveCount(1);
            receivedOrders.First().Status.Should().Be(OrderStatusEnum.RECEIVED);
        }

        [Fact]
        public async Task OrdersDbContext_CanIncludeOrderItems()
        {
            using var context = CreateContext();
            var order = new Order
            {
                CustomerId = Guid.NewGuid() ,
                Number = 109,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        ProductName = "Produto 1",
                        Quantity = 2,
                        UnitPrice = 10.00m,
                        CreatedAt = DateTime.UtcNow
                    },
                    new OrderItem
                    {
                        ProductId = 2,
                        ProductName = "Produto 2",
                        Quantity = 1,
                        UnitPrice = 20.00m,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var orderWithItems = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Number == 109);

            orderWithItems.Should().NotBeNull();
            orderWithItems!.Items.Should().HaveCount(2);
        }

        #endregion

        #region SaveChanges Tests

        [Fact]
        public async Task OrdersDbContext_SaveChangesAsync_ReturnsSavedEntitiesCount()
        {
            using var context = CreateContext();
            var order = new Order
            {
                CustomerId = Guid.NewGuid(),
                Number = 110,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);

            var result = await context.SaveChangesAsync();

            result.Should().BeGreaterThan(0);
        }

        #endregion

        #region DbSet Tests

        [Fact]
        public void OrdersDbContext_OrdersDbSet_IsNotNull()
        {
            using var context = CreateContext();

            context.Orders.Should().NotBeNull();
            context.Orders.Should().BeAssignableTo<DbSet<Order>>();
        }

        [Fact]
        public void OrdersDbContext_OrderItemsDbSet_IsNotNull()
        {
            using var context = CreateContext();

            context.OrderItems.Should().NotBeNull();
            context.OrderItems.Should().BeAssignableTo<DbSet<OrderItem>>();
        }

        #endregion
    }
}