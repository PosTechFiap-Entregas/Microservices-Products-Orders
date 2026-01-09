using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Orders.Infrastructure.HttpClients;
using Xunit;

namespace Orders.Tests.Infrastructure.HttpClients
{
    public class PaymentHttpClientTests
    {
        private static HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
        {
            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }

        [Fact]
        public async Task CreatePaymentAsync_ReturnsPayment_WhenResponseIs200_AndRequestIsCamelCase()
        {
            // Arrange
            var payment = new PaymentResponse("pay_1", "1", 50.00m, "PENDING", "qr", DateTime.UtcNow);
            var json = JsonSerializer.Serialize(payment);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req =>
                       req.Method == HttpMethod.Post
                       && req.RequestUri!.PathAndQuery == "/paymentservice/v1/payments"
                       && req.Content != null
                       && req.Content.ReadAsStringAsync().Result.Contains("\"orderId\"")
                       && req.Content.ReadAsStringAsync().Result.Contains("\"totalAmount\"")
                   ),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = CreateHttpClient(handlerMock);
            var loggerMock = new Mock<ILogger<PaymentHttpClient>>();
            var client = new PaymentHttpClient(httpClient, loggerMock.Object);

            // Act
            var result = await client.CreatePaymentAsync("1", 50.00m);

            // Assert
            result.Should().NotBeNull();
            result!.PaymentId.Should().Be(payment.PaymentId);
            result.OrderId.Should().Be(payment.OrderId);
            result.TotalAmount.Should().Be(payment.TotalAmount);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

            // Verify informational logging occurred (create + success)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Criando pagamento")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Pagamento criado com sucesso")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreatePaymentAsync_ReturnsNull_AndLogsWarning_WhenResponseIsNotSuccess()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = CreateHttpClient(handlerMock);
            var loggerMock = new Mock<ILogger<PaymentHttpClient>>();
            var client = new PaymentHttpClient(httpClient, loggerMock.Object);

            // Act
            var result = await client.CreatePaymentAsync("999", 1.00m);

            // Assert
            result.Should().BeNull();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Falha ao criar pagamento")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task CreatePaymentAsync_ThrowsException_WhenHttpClientThrows_AndLogsError()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException("Network"))
               .Verifiable();

            var httpClient = CreateHttpClient(handlerMock);
            var loggerMock = new Mock<ILogger<PaymentHttpClient>>();
            var client = new PaymentHttpClient(httpClient, loggerMock.Object);

            // Act
            Func<Task> act = async () => await client.CreatePaymentAsync("1", 10.00m);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Erro ao comunicar com o serviço de pagamento*");

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao criar pagamento")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public void PaymentHttpClient_Implements_Interface()
        {
            // Arrange / Act
            var httpClient = new HttpClient();
            var loggerMock = new Mock<ILogger<PaymentHttpClient>>();
            var client = new PaymentHttpClient(httpClient, loggerMock.Object);

            // Assert
            client.Should().BeAssignableTo<IPaymentHttpClient>();
        }
    }
}