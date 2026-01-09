using System.Text.Json;
using FluentAssertions;
using Orders.Infrastructure.HttpClients;
using Xunit;

namespace Orders.Tests.Infrastructure.HttpClients
{
    public class PaymentRequestTests
    {
        [Fact]
        public void PaymentRequest_Record_ShouldStoreValues_AndEqualityWorks()
        {
            var req1 = new PaymentRequest("1", 10.50m);
            var req2 = new PaymentRequest("1", 10.50m);
            var req3 = new PaymentRequest("2", 5.00m);

            req1.OrderId.Should().Be("1");
            req1.TotalAmount.Should().Be(10.50m);

            req1.Should().Be(req2);
            req1.Should().NotBe(req3);
        }

        [Fact]
        public void PaymentRequest_Serialization_UsesCamelCase_WhenConfigured()
        {
            var req = new PaymentRequest("42", 99.90m);
            var json = JsonSerializer.Serialize(req, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            json.Should().Contain("\"orderId\"");
            json.Should().Contain("\"totalAmount\"");
            json.Should().Contain("42");
            json.Should().Contain("99.9");
        }
    }
}