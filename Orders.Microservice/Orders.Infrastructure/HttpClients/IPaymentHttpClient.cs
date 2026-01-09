using System.Threading.Tasks;

namespace Orders.Infrastructure.HttpClients
{
    public interface IPaymentHttpClient
    {
        Task<PaymentResponse?> CreatePaymentAsync(string orderId, decimal totalAmount);
    }
}