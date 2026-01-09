using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orders.Infrastructure.HttpClients
{
    public class PaymentHttpClient : IPaymentHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentHttpClient> _logger;

        public PaymentHttpClient(HttpClient httpClient, ILogger<PaymentHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PaymentResponse?> CreatePaymentAsync(string orderId, decimal totalAmount)
        {
            try
            {
                var request = new PaymentRequest(orderId, totalAmount);

                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation(
                    "Criando pagamento para Order {OrderId} no valor de {TotalAmount}",
                    orderId, totalAmount);

                var response = await _httpClient.PostAsync("/paymentservice/v1/payments", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Falha ao criar pagamento. OrderId: {OrderId}, Status: {Status}",
                        orderId, response.StatusCode);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var payment = JsonSerializer.Deserialize<PaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation(
                    "Pagamento criado com sucesso. OrderId: {OrderId}, PaymentId: {PaymentId}",
                    orderId, payment?.PaymentId);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pagamento para Order {OrderId}", orderId);
                throw new Exception($"Erro ao comunicar com o serviço de pagamento: {ex.Message}", ex);
            }
        }
    }
}