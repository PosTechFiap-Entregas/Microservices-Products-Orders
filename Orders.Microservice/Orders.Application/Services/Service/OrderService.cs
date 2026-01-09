using Orders.Application.DTOs;
using Orders.Application.Services.Interface;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Interfaces.Repository;
using Orders.Infrastructure.HttpClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.Services.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IProductsHttpClient _productsClient;
        private readonly IPaymentHttpClient _paymentClient;

        public OrderService(IOrderRepository repository, IProductsHttpClient productsClient, IPaymentHttpClient paymentClient)
        {
            _repository = repository;
            _productsClient = productsClient;
            _paymentClient = paymentClient;
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _repository.GetByIdWithItemsAsync(id);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _repository.GetAllAsync();
            return orders.Select(MapToDto);
        }

        public async Task<IEnumerable<OrderDto>> GetActiveOrdersAsync()
        {
            var orders = await _repository.GetActiveOrdersAsync();
            return orders.Select(MapToDto);
        }

        public async Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatusEnum status)
        {
            var orders = await _repository.GetByStatusAsync(status);
            return orders.Select(MapToDto);
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("O pedido deve conter ao menos um item");

            var orderNumber = await _repository.GetNextOrderNumberAsync();

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                Observation = dto.Observation,
                Number = orderNumber,
                Status = OrderStatusEnum.RECEIVED,
                PaymentStatus = PaymentStatusEnum.PENDING,
            };

            foreach (var itemDto in dto.Items)
            {
                var product = await _productsClient.GetProductByIdAsync(itemDto.ProductId);

                if (product == null)
                    throw new KeyNotFoundException($"Produto com ID {itemDto.ProductId} não encontrado");

                if (!product.Active)
                    throw new InvalidOperationException($"Produto {product.Name} não está ativo");

                var item = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                };

                order.AddItem(item);
            }

            order.RecalculateTotal();

            var created = await _repository.AddAsync(order);

            //Tentar criar pagamento após criar a order
            try
            {
                var payment = await _paymentClient.CreatePaymentAsync(
                    created.Id.ToString(),
                    created.TotalAmount);

                if (payment != null)
                {
                    created.SetPaymentId(payment.PaymentId);
                    await _repository.UpdateAsync(created);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar pagamento: {ex.Message}");
            }

            return MapToDto(created);
        }

        public async Task<OrderDto> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            var order = await _repository.GetByIdWithItemsAsync(id)
                ?? throw new KeyNotFoundException($"Pedido com ID {id} não encontrado");

            order.ChangeStatus(dto.Status);

            var updated = await _repository.UpdateAsync(order);
            return MapToDto(updated);
        }

        public async Task<OrderDto> SetPaymentIdAsync(int id, SetPaymentIdDto dto)
        {
            var order = await _repository.GetByIdWithItemsAsync(id)
                ?? throw new KeyNotFoundException($"Pedido com ID {id} não encontrado");

            order.SetPaymentId(dto.PaymentId);

            var updated = await _repository.UpdateAsync(order);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static OrderDto MapToDto(Order order) => new(
            order.Id,
            order.CustomerId,
            order.Status,
            order.Observation,
            order.Number,
            order.PaymentId,
            order.PaymentStatus,
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.Subtotal
            )).ToList()
        );
    }
}