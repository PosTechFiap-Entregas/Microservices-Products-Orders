using FluentAssertions;
using Orders.Domain.Enums;

namespace Orders.Tests.Domain.Enums
{
    public class PaymentStatusEnumTests
    {
        #region Enum Values Tests

        [Fact]
        public void PaymentStatusEnum_ShouldHavePendingValue()
        {
            var status = PaymentStatusEnum.PENDING;

            status.Should().Be(PaymentStatusEnum.PENDING);
            ((int)status).Should().Be(0);
        }

        [Fact]
        public void PaymentStatusEnum_ShouldHavePaidValue()
        {
            var status = PaymentStatusEnum.PAID;

            status.Should().Be(PaymentStatusEnum.PAID);
            ((int)status).Should().Be(1);
        }

        [Fact]
        public void PaymentStatusEnum_ShouldHaveRefusedValue()
        {
            var status = PaymentStatusEnum.REFUSED;

            status.Should().Be(PaymentStatusEnum.REFUSED);
            ((int)status).Should().Be(2);
        }

        [Fact]
        public void PaymentStatusEnum_ShouldHaveCancelledValue()
        {
            var status = PaymentStatusEnum.CANCELLED;

            status.Should().Be(PaymentStatusEnum.CANCELLED);
            ((int)status).Should().Be(3);
        }

        #endregion

        #region Enum Count and Values Tests

        [Fact]
        public void PaymentStatusEnum_ShouldHaveExactlyFourValues()
        {
            var values = Enum.GetValues(typeof(PaymentStatusEnum));

            values.Length.Should().Be(4);
        }

        [Fact]
        public void PaymentStatusEnum_ShouldContainAllExpectedValues()
        {
            var values = Enum.GetValues(typeof(PaymentStatusEnum)).Cast<PaymentStatusEnum>();

            values.Should().Contain(PaymentStatusEnum.PENDING);
            values.Should().Contain(PaymentStatusEnum.PAID);
            values.Should().Contain(PaymentStatusEnum.REFUSED);
            values.Should().Contain(PaymentStatusEnum.CANCELLED);
        }

        [Theory]
        [InlineData(0, PaymentStatusEnum.PENDING)]
        [InlineData(1, PaymentStatusEnum.PAID)]
        [InlineData(2, PaymentStatusEnum.REFUSED)]
        [InlineData(3, PaymentStatusEnum.CANCELLED)]
        public void PaymentStatusEnum_ShouldMapIntegerToCorrectValue(int intValue, PaymentStatusEnum expected)
        {
            var status = (PaymentStatusEnum)intValue;

            status.Should().Be(expected);
        }

        #endregion

        #region Enum Name Tests

        [Fact]
        public void PaymentStatusEnum_PendingShouldHaveCorrectName()
        {
            var name = PaymentStatusEnum.PENDING.ToString();

            name.Should().Be("PENDING");
        }

        [Fact]
        public void PaymentStatusEnum_PaidShouldHaveCorrectName()
        {
            var name = PaymentStatusEnum.PAID.ToString();

            name.Should().Be("PAID");
        }

        [Fact]
        public void PaymentStatusEnum_RefusedShouldHaveCorrectName()
        {
            var name = PaymentStatusEnum.REFUSED.ToString();

            name.Should().Be("REFUSED");
        }

        [Fact]
        public void PaymentStatusEnum_CancelledShouldHaveCorrectName()
        {
            var name = PaymentStatusEnum.CANCELLED.ToString();

            name.Should().Be("CANCELLED");
        }

        #endregion

        #region Parse and TryParse Tests

        [Theory]
        [InlineData("PENDING", PaymentStatusEnum.PENDING)]
        [InlineData("PAID", PaymentStatusEnum.PAID)]
        [InlineData("REFUSED", PaymentStatusEnum.REFUSED)]
        [InlineData("CANCELLED", PaymentStatusEnum.CANCELLED)]
        public void PaymentStatusEnum_ShouldParseStringCorrectly(string statusString, PaymentStatusEnum expected)
        {
            var status = Enum.Parse<PaymentStatusEnum>(statusString);

            status.Should().Be(expected);
        }

        [Fact]
        public void PaymentStatusEnum_ShouldParseStringIgnoreCase()
        {
            var status = Enum.Parse<PaymentStatusEnum>("paid", ignoreCase: true);

            status.Should().Be(PaymentStatusEnum.PAID);
        }

        [Fact]
        public void PaymentStatusEnum_TryParse_WithValidValue_ReturnsTrue()
        {
            var result = Enum.TryParse<PaymentStatusEnum>("REFUSED", out var status);

            result.Should().BeTrue();
            status.Should().Be(PaymentStatusEnum.REFUSED);
        }

        [Fact]
        public void PaymentStatusEnum_TryParse_WithInvalidValue_ReturnsFalse()
        {
            var result = Enum.TryParse<PaymentStatusEnum>("INVALID_STATUS", out var status);

            result.Should().BeFalse();
            status.Should().Be(default(PaymentStatusEnum));
        }

        #endregion

        #region IsDefined Tests

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, false)]
        [InlineData(99, false)]
        [InlineData(-1, false)]
        public void PaymentStatusEnum_IsDefined_ShouldValidateCorrectly(int value, bool expectedResult)
        {
            var isDefined = Enum.IsDefined(typeof(PaymentStatusEnum), value);

            isDefined.Should().Be(expectedResult);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void PaymentStatusEnum_DefaultValue_ShouldBePending()
        {
            var defaultStatus = default(PaymentStatusEnum);

            defaultStatus.Should().Be(PaymentStatusEnum.PENDING);
            ((int)defaultStatus).Should().Be(0);
        }

        #endregion

        #region Comparison Tests

        [Fact]
        public void PaymentStatusEnum_ShouldCompareCorrectly()
        {
            var pending = PaymentStatusEnum.PENDING;
            var paid = PaymentStatusEnum.PAID;
            var refused = PaymentStatusEnum.REFUSED;
            var cancelled = PaymentStatusEnum.CANCELLED;

            (pending < paid).Should().BeTrue();
            (paid < refused).Should().BeTrue();
            (refused < cancelled).Should().BeTrue();
            (cancelled > pending).Should().BeTrue();
        }

        [Fact]
        public void PaymentStatusEnum_EqualityComparison_ShouldWork()
        {
            var status1 = PaymentStatusEnum.PAID;
            var status2 = PaymentStatusEnum.PAID;
            var status3 = PaymentStatusEnum.PENDING;

            (status1 == status2).Should().BeTrue();
            (status1 != status3).Should().BeTrue();
            status1.Equals(status2).Should().BeTrue();
            status1.Equals(status3).Should().BeFalse();
        }

        #endregion

        #region GetNames and GetValues Tests

        [Fact]
        public void PaymentStatusEnum_GetNames_ShouldReturnAllNames()
        {
            var names = Enum.GetNames(typeof(PaymentStatusEnum));

            names.Should().HaveCount(4);
            names.Should().Contain("PENDING");
            names.Should().Contain("PAID");
            names.Should().Contain("REFUSED");
            names.Should().Contain("CANCELLED");
        }

        [Fact]
        public void PaymentStatusEnum_GetValues_ShouldReturnInCorrectOrder()
        {
            var values = Enum.GetValues(typeof(PaymentStatusEnum)).Cast<PaymentStatusEnum>().ToList();

            values[0].Should().Be(PaymentStatusEnum.PENDING);
            values[1].Should().Be(PaymentStatusEnum.PAID);
            values[2].Should().Be(PaymentStatusEnum.REFUSED);
            values[3].Should().Be(PaymentStatusEnum.CANCELLED);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public void PaymentStatusEnum_SuccessfulStates_ShouldBePaid()
        {
            var successStates = new[] { PaymentStatusEnum.PAID };

            successStates.Should().Contain(PaymentStatusEnum.PAID);
            successStates.Should().NotContain(PaymentStatusEnum.PENDING);
            successStates.Should().NotContain(PaymentStatusEnum.REFUSED);
            successStates.Should().NotContain(PaymentStatusEnum.CANCELLED);
        }

        [Fact]
        public void PaymentStatusEnum_FailedStates_ShouldBeRefusedOrCancelled()
        {
            var failedStates = new[] { PaymentStatusEnum.REFUSED, PaymentStatusEnum.CANCELLED };

            failedStates.Should().Contain(PaymentStatusEnum.REFUSED);
            failedStates.Should().Contain(PaymentStatusEnum.CANCELLED);
            failedStates.Should().NotContain(PaymentStatusEnum.PAID);
            failedStates.Should().NotContain(PaymentStatusEnum.PENDING);
        }

        #endregion
    }
}