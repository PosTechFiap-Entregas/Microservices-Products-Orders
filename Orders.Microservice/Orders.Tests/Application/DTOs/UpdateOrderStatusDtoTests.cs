using FluentAssertions;
using Orders.Application.DTOs;
using Orders.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Tests.Application.DTOs
{
    public class UpdateOrderStatusDtoTests
    {
        [Fact]
        public void UpdateOrderStatusDto_WithReceivedStatus_ShouldCreateSuccessfully()
        {
            var status = OrderStatusEnum.RECEIVED;

            var dto = new UpdateOrderStatusDto(status);

            dto.Should().NotBeNull();
            dto.Status.Should().Be(status);
        }

        [Theory]
        [InlineData(OrderStatusEnum.RECEIVED)]
        [InlineData(OrderStatusEnum.IN_PREPARATION)]
        [InlineData(OrderStatusEnum.READY)]
        [InlineData(OrderStatusEnum.FINALIZED)]
        public void UpdateOrderStatusDto_WithDifferentStatuses_ShouldCreateSuccessfully(OrderStatusEnum status)
        {
            var dto = new UpdateOrderStatusDto(status);

            dto.Status.Should().Be(status);
        }

        [Fact]
        public void UpdateOrderStatusDto_EqualityComparison_ShouldWorkCorrectly()
        {
            var dto1 = new UpdateOrderStatusDto(OrderStatusEnum.READY);
            var dto2 = new UpdateOrderStatusDto(OrderStatusEnum.READY);

            dto1.Should().Be(dto2);
        }

        [Fact]
        public void UpdateOrderStatusDto_DifferentStatuses_ShouldNotBeEqual()
        {
            var dto1 = new UpdateOrderStatusDto(OrderStatusEnum.RECEIVED);
            var dto2 = new UpdateOrderStatusDto(OrderStatusEnum.FINALIZED);

            dto1.Should().NotBe(dto2);
        }
    }
}