using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Orders.Infrastructure.HttpClients;
using System.Net;
using System.Text;

namespace Orders.Tests.Infrastructure.HttpClients
{
    public class ProductsHttpClientTests
    {
        private static HttpClient CreateHttpClient(HttpResponseMessage responseMessage, Mock<HttpMessageHandler>? handlerMock = null)
        {
            if (handlerMock == null)
            {
                handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                handlerMock
                   .Protected()
                   .Setup<Task<HttpResponseMessage>>(
                       "SendAsync",
                       ItExpr.IsAny<HttpRequestMessage>(),
                       ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(responseMessage)
                   .Verifiable();
            }

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new System.Uri("http://localhost")
            };

            return client;
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenResponseIs200()
        {
            var productResponse = new ProductResponse(1, "Produto Teste", 12.50m, "Categoria", "Desc", true, "img");
            var json = System.Text.Json.JsonSerializer.Serialize(productResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.PathAndQuery.StartsWith("/api/products/")),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = CreateHttpClient(responseMessage, handlerMock);
            var loggerMock = new Mock<ILogger<ProductsHttpClient>>();
            var client = new ProductsHttpClient(httpClient, loggerMock.Object);

            var result = await client.GetProductByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(productResponse.Id);
            result.Name.Should().Be(productResponse.Name);
            result.Price.Should().Be(productResponse.Price);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri!.PathAndQuery.StartsWith("/api/products/")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsNull_AndLogsWarning_WhenNotFound()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = CreateHttpClient(responseMessage, handlerMock);
            var loggerMock = new Mock<ILogger<ProductsHttpClient>>();
            var client = new ProductsHttpClient(httpClient, loggerMock.Object);

            var result = await client.GetProductByIdAsync(999);

            result.Should().BeNull();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Produto")),
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
        public async Task GetProductByIdAsync_ThrowsException_WhenHttpClientThrows()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException("Network error"))
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new System.Uri("http://localhost")
            };

            var loggerMock = new Mock<ILogger<ProductsHttpClient>>();
            var client = new ProductsHttpClient(httpClient, loggerMock.Object);

            Func<Task> act = async () => await client.GetProductByIdAsync(1);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Erro ao comunicar com o serviço de produtos*");

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenContentIsJsonNull_ReturnsNull_WithoutLoggingError()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = CreateHttpClient(responseMessage, handlerMock);
            var loggerMock = new Mock<ILogger<ProductsHttpClient>>();
            var client = new ProductsHttpClient(httpClient, loggerMock.Object);

            var result = await client.GetProductByIdAsync(5);

            result.Should().BeNull();

            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenResponseJsonIsInvalid_ThrowsException_AndLogsError()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid-json ", Encoding.UTF8, "application/json")
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = CreateHttpClient(responseMessage, handlerMock);
            var loggerMock = new Mock<ILogger<ProductsHttpClient>>();
            var client = new ProductsHttpClient(httpClient, loggerMock.Object);

            Func<Task> act = async () => await client.GetProductByIdAsync(7);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Erro ao comunicar com o serviço de produtos*");

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao buscar produto")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}