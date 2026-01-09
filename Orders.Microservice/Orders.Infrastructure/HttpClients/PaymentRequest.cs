namespace Orders.Infrastructure.HttpClients
{
    public record PaymentRequest(
        string OrderId,
        decimal TotalAmount
    );
}