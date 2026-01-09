using System;
using FluentAssertions;
using Orders.Infrastructure.HttpClients;
using Xunit;

namespace Orders.Tests.Infrastructure.HttpClients
{
    public class PaymentResponseTests
    {
        [Fact]
        public void PaymentResponse_Record_ShouldStoreValues_AndEqualityWorks()
        {
            var now = DateTime.UtcNow;
            var r1 = new PaymentResponse("pay1", "1", 10.00m, "PENDING", "qr", now);
            var r2 = new PaymentResponse("pay1", "1", 10.00m, "PENDING", "qr", now);
            var r3 = new PaymentResponse("pay2", "2", 5.00m, "PAID", "qr2", now);

            r1.PaymentId.Should().Be("pay1");
            r1.OrderId.Should().Be("1");
            r1.TotalAmount.Should().Be(10.00m);
            r1.Status.Should().Be("PENDING");
            r1.QrCode.Should().Be("qr");
            r1.CreatedAt.Should().Be(now);

            r1.Should().Be(r2);
            r1.Should().NotBe(r3);
        }
    }
}