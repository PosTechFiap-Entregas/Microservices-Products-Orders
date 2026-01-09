using FluentAssertions;
using Orders.Infrastructure.HttpClients;
using Xunit;

namespace Orders.Tests.Infrastructure.HttpClients
{
    public class ProductResponseTests
    {
        [Fact]
        public void ProductResponse_Record_ShouldStoreValues_AndEqualityWorks()
        {
            var r1 = new ProductResponse(1, "P", 10.00m, "Cat", "Desc", true, "img");
            var r2 = new ProductResponse(1, "P", 10.00m, "Cat", "Desc", true, "img");
            var r3 = new ProductResponse(2, "Q", 5.00m, "Cat2", null, false, null);

            r1.Id.Should().Be(1);
            r1.Name.Should().Be("P");
            r1.Price.Should().Be(10.00m);
            r1.Category.Should().Be("Cat");
            r1.Description.Should().Be("Desc");
            r1.Active.Should().BeTrue();
            r1.ImageUrl.Should().Be("img");

            r1.Should().Be(r2);
            r1.Should().NotBe(r3);
        }
    }
}