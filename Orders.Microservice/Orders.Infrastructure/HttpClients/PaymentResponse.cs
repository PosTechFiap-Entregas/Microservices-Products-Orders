using System;

namespace Orders.Infrastructure.HttpClients
{
    public record PaymentResponse(
        string PaymentId,
        string OrderId,
        decimal TotalAmount,
        string Status,
        string QrCode,
        DateTime CreatedAt
    );
}