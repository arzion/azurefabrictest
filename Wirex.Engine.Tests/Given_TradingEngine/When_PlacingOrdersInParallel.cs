using System.Linq;
using FluentAssertions;
using Xunit;

namespace Wirex.Engine.Tests.Given_TradingEngine
{
    // ReSharper disable once InconsistentNaming
    public class When_PlacingOrdersInParalell : IClassFixture<RandomOrdersProcessedInParallelClassFixture>
    {
        private readonly RandomOrdersProcessedInParallelClassFixture _context;

        public When_PlacingOrdersInParalell(RandomOrdersProcessedInParallelClassFixture classFixture)
        {
            _context = classFixture;
        }

        [Fact]
        public void Then_AllOrdersWereOpened()
        {
            // Assert
            _context.ActualOpenedOrders.Should().NotBeEmpty().And.HaveSameCount(_context.Orders);
        }

        [Fact]
        public void Then_MoreThanHalfOfOrdersWereClosed()
        {
            // Assert
            _context.ActualClosedOrders.Count.Should().BeGreaterThan(_context.ActualOpenedOrders.Count / 2);
        }

        [Fact]
        public void Then_NoMoreOrdersThatCanBeClosed()
        {
            // Assert
            var notClosedOrders = _context.ActualOpenedOrders.Except(_context.ActualClosedOrders).ToList();

            var minSellPrice = notClosedOrders.Where(it => it.Side == Side.Sell).Min(it => it.Price);
            var maxBuyPrice = notClosedOrders.Where(it => it.Side == Side.Buy).Max(it => it.Price);

            minSellPrice.Should().BeGreaterThan(maxBuyPrice);
        }
    }
}