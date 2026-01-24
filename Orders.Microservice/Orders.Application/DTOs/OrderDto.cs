using Orders.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.DTOs
{
    public record OrderDto(
        Guid Id,
        int? CustomerId,
        OrderStatusEnum Status,
        string? Observation,
        int Number,
        string? PaymentId,
        PaymentStatusEnum PaymentStatus,
        decimal Total,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        List<OrderItemDto> Items
    );
}