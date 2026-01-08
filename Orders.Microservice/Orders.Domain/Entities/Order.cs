using Orders.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orders.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public OrderStatusEnum Status { get; set; } = OrderStatusEnum.RECEIVED;
        public string? Observation { get; set; }
        public int Number { get; set; }
        public string? PaymentId { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.PENDING;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal CalculateTotal()
        {
            return Items.Sum(item => item.Quantity * item.UnitPrice);
        }

        public void ChangeStatus(OrderStatusEnum newStatus)
        {
            if (!IsValidStatusTransition(Status, newStatus))
                throw new InvalidOperationException($"Não é possível mudar de {Status} para {newStatus}");

            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddItem(OrderItem item)
        {
            if (Status != OrderStatusEnum.RECEIVED)
                throw new InvalidOperationException("Não é possível adicionar itens");

            Items.Add(item);

            RecalculateTotal();

            UpdatedAt = DateTime.UtcNow;
        }

        public void RecalculateTotal()
        {
            TotalAmount = CalculateTotal();
        }

        public void SetPaymentId(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                throw new ArgumentException("PaymentId não pode ser vazio");

            PaymentId = paymentId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ProcessPayment(string paymentId, PaymentStatusEnum paymentStatus)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                throw new ArgumentException("PaymentId não pode ser vazio");

            PaymentId = paymentId;
            PaymentStatus = paymentStatus;

            if (paymentStatus == PaymentStatusEnum.PAID && Status == OrderStatusEnum.RECEIVED)
            {
                Status = OrderStatusEnum.IN_PREPARATION;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void CancelDueToPaymentFailure()
        {
            if (PaymentStatus == PaymentStatusEnum.REFUSED ||
                PaymentStatus == PaymentStatusEnum.CANCELLED)
            {
                UpdatedAt = DateTime.UtcNow;
            }
        }

        private bool IsValidStatusTransition(OrderStatusEnum current, OrderStatusEnum next)
        {
            return (current, next) switch
            {
                (OrderStatusEnum.RECEIVED, OrderStatusEnum.IN_PREPARATION) => true,
                (OrderStatusEnum.IN_PREPARATION, OrderStatusEnum.READY) => true,
                (OrderStatusEnum.READY, OrderStatusEnum.FINALIZED) => true,
                _ => false
            };
        }
    }
}