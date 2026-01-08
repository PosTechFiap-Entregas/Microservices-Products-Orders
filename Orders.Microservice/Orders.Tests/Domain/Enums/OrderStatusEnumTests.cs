using FluentAssertions;
using Orders.Domain.Enums;

namespace Orders.Tests.Domain.Enums
{
    public class OrderStatusEnumTests
    {
        #region Enum Values Tests

        [Fact]
        public void OrderStatusEnum_ShouldHaveReceivedValue()
        {
            var status = OrderStatusEnum.RECEIVED;

            status.Should().Be(OrderStatusEnum.RECEIVED);
            ((int)status).Should().Be(1);
        }

        [Fact]
        public void OrderStatusEnum_ShouldHaveInPreparationValue()
        {
            var status = OrderStatusEnum.IN_PREPARATION;

            status.Should().Be(OrderStatusEnum.IN_PREPARATION);
            ((int)status).Should().Be(2);
        }

        [Fact]
        public void OrderStatusEnum_ShouldHaveReadyValue()
        {
            var status = OrderStatusEnum.READY;

            status.Should().Be(OrderStatusEnum.READY);
            ((int)status).Should().Be(3);
        }

        [Fact]
        public void OrderStatusEnum_ShouldHaveFinalizedValue()
        {
            var status = OrderStatusEnum.FINALIZED;

            status.Should().Be(OrderStatusEnum.FINALIZED);
            ((int)status).Should().Be(4);
        }

        #endregion

        #region Enum Count and Values Tests

        [Fact]
        public void OrderStatusEnum_ShouldHaveExactlyFourValues()
        {
            var values = Enum.GetValues(typeof(OrderStatusEnum));

            ((Array)values).Length.Should().Be(4);
        }

        [Fact]
        public void OrderStatusEnum_ShouldContainAllExpectedValues()
        {
            var values = Enum.GetValues(typeof(OrderStatusEnum)).Cast<OrderStatusEnum>();

            values.Should().Contain(OrderStatusEnum.RECEIVED);
            values.Should().Contain(OrderStatusEnum.IN_PREPARATION);
            values.Should().Contain(OrderStatusEnum.READY);
            values.Should().Contain(OrderStatusEnum.FINALIZED);
        }

        [Theory]
        [InlineData(1, OrderStatusEnum.RECEIVED)]
        [InlineData(2, OrderStatusEnum.IN_PREPARATION)]
        [InlineData(3, OrderStatusEnum.READY)]
        [InlineData(4, OrderStatusEnum.FINALIZED)]
        public void OrderStatusEnum_ShouldMapIntegerToCorrectValue(int intValue, OrderStatusEnum expected)
        {
            var status = (OrderStatusEnum)intValue;

            status.Should().Be(expected);
        }

        #endregion

        #region Enum Name Tests

        [Fact]
        public void OrderStatusEnum_ReceivedShouldHaveCorrectName()
        {
            var name = OrderStatusEnum.RECEIVED.ToString();

            name.Should().Be("RECEIVED");
        }

        [Fact]
        public void OrderStatusEnum_InPreparationShouldHaveCorrectName()
        {
            var name = OrderStatusEnum.IN_PREPARATION.ToString();

            name.Should().Be("IN_PREPARATION");
        }

        [Fact]
        public void OrderStatusEnum_ReadyShouldHaveCorrectName()
        {
            var name = OrderStatusEnum.READY.ToString();

            name.Should().Be("READY");
        }

        [Fact]
        public void OrderStatusEnum_FinalizedShouldHaveCorrectName()
        {
            var name = OrderStatusEnum.FINALIZED.ToString();

            name.Should().Be("FINALIZED");
        }

        #endregion

        #region Parse and TryParse Tests

        [Theory]
        [InlineData("RECEIVED", OrderStatusEnum.RECEIVED)]
        [InlineData("IN_PREPARATION", OrderStatusEnum.IN_PREPARATION)]
        [InlineData("READY", OrderStatusEnum.READY)]
        [InlineData("FINALIZED", OrderStatusEnum.FINALIZED)]
        public void OrderStatusEnum_ShouldParseStringCorrectly(string statusString, OrderStatusEnum expected)
        {
            var status = Enum.Parse<OrderStatusEnum>(statusString);

            status.Should().Be(expected);
        }

        [Fact]
        public void OrderStatusEnum_ShouldParseStringIgnoreCase()
        {
            var status = Enum.Parse<OrderStatusEnum>("received", ignoreCase: true);

            status.Should().Be(OrderStatusEnum.RECEIVED);
        }

        [Fact]
        public void OrderStatusEnum_TryParse_WithValidValue_ReturnsTrue()
        {
            var result = Enum.TryParse<OrderStatusEnum>("READY", out var status);

            result.Should().BeTrue();
            status.Should().Be(OrderStatusEnum.READY);
        }

        [Fact]
        public void OrderStatusEnum_TryParse_WithInvalidValue_ReturnsFalse()
        {
            var result = Enum.TryParse<OrderStatusEnum>("INVALID_STATUS", out var status);

            result.Should().BeFalse();
            status.Should().Be(default(OrderStatusEnum));
        }

        #endregion

        #region IsDefined Tests

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, true)]
        [InlineData(0, false)]
        [InlineData(5, false)]
        [InlineData(99, false)]
        public void OrderStatusEnum_IsDefined_ShouldValidateCorrectly(int value, bool expectedResult)
        {
            var isDefined = Enum.IsDefined(typeof(OrderStatusEnum), value);

            isDefined.Should().Be(expectedResult);
        }

        #endregion

        #region Comparison Tests

        [Fact]
        public void OrderStatusEnum_ShouldCompareCorrectly()
        {
            var received = OrderStatusEnum.RECEIVED;
            var inPreparation = OrderStatusEnum.IN_PREPARATION;
            var ready = OrderStatusEnum.READY;
            var finalized = OrderStatusEnum.FINALIZED;

            (received < inPreparation).Should().BeTrue();
            (inPreparation < ready).Should().BeTrue();
            (ready < finalized).Should().BeTrue();
            (finalized > received).Should().BeTrue();
        }

        [Fact]
        public void OrderStatusEnum_EqualityComparison_ShouldWork()
        {
            var status1 = OrderStatusEnum.RECEIVED;
            var status2 = OrderStatusEnum.RECEIVED;
            var status3 = OrderStatusEnum.IN_PREPARATION;

            (status1 == status2).Should().BeTrue();
            (status1 != status3).Should().BeTrue();
            status1.Equals(status2).Should().BeTrue();
            status1.Equals(status3).Should().BeFalse();
        }

        #endregion

        #region GetNames and GetValues Tests

        [Fact]
        public void OrderStatusEnum_GetNames_ShouldReturnAllNames()
        {
            var names = Enum.GetNames(typeof(OrderStatusEnum));

            names.Should().HaveCount(4);
            names.Should().Contain("RECEIVED");
            names.Should().Contain("IN_PREPARATION");
            names.Should().Contain("READY");
            names.Should().Contain("FINALIZED");
        }

        [Fact]
        public void OrderStatusEnum_GetValues_ShouldReturnInCorrectOrder()
        {
            var values = Enum.GetValues(typeof(OrderStatusEnum)).Cast<OrderStatusEnum>().ToList();

            values[0].Should().Be(OrderStatusEnum.RECEIVED);
            values[1].Should().Be(OrderStatusEnum.IN_PREPARATION);
            values[2].Should().Be(OrderStatusEnum.READY);
            values[3].Should().Be(OrderStatusEnum.FINALIZED);
        }

        #endregion
    }
}