using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.DTOs
{
    public record CreateOrderDto(
        Guid? CustomerId,
        string? Observation,
        List<CreateOrderItemDto> Items
    );
}