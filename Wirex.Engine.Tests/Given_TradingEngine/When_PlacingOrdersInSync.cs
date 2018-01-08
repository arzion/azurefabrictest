using System.Linq;
using FluentAssertions;
using Xunit;

namespace Wirex.Engine.Tests.Given_TradingEngine
{
    // ReSharper disable once InconsistentNaming
    public class When_PlacingOrdersInSync : IClassFixture<RandomOrdersProcessedInSyncClassFixture>
    {
        private readonly RandomOrdersProcessedInSyncClassFixture _context;

        public When_PlacingOrdersInSync(RandomOrdersProcessedInSyncClassFixture classFixture)
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
        public void Then_ProfitShouldBePositiveForAllClosedOrders()
        {
            // Assert
            _context.Profit.Should().BePositive();

            decimal notClosedOrdersProfit = 0;
            var notClosedOrders = _context.ActualOpenedOrders.Except(_context.ActualClosedOrders).ToList();
            foreach (var notClosedOrder in notClosedOrders)
            {
                var soldAmount = notClosedOrder.Price * (notClosedOrder.Amount - notClosedOrder.RemainingAmount);
                if (notClosedOrder.Side == Side.Buy)
                {
                    notClosedOrdersProfit += soldAmount;
                }
                if (notClosedOrder.Side == Side.Sell)
                {
                    notClosedOrdersProfit -= soldAmount;
                }
            }

            notClosedOrdersProfit.Should().BePositive();
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