using System.Collections.Generic;
using Wirex.Engine.Engine;
using Wirex.Engine.Services;

namespace Wirex.Engine.Tests.Given_TradingEngine
{
    public class RandomOrdersProcessedInSyncClassFixture : ClassFixtureBase
    {
        public IList<Order> Orders { get; set; }
        public IList<Order> ActualOpenedOrders { get; set; }
        public IList<Order> ActualClosedOrders { get; set; }

        public ITradingEngine Sut { get; set; }

        public decimal Profit { get; set; }

        public RandomOrdersProcessedInSyncClassFixture()
        {
            // Arrange
            Orders = GetRandomOrders();
            ActualOpenedOrders = new List<Order>();
            ActualClosedOrders = new List<Order>();

            Sut = new QueuedTradingEngine(new OrdersMatchingRule());

            Sut.OrderOpened += (sender, args) => { ActualOpenedOrders.Add(args.Order); };
            Sut.OrderClosed += (sender, args) => { ActualClosedOrders.Add(args.Order); };

            Sut.OrderClosed += (sender, args) =>
            {
                var order = args.Order;
                var totalAmount = order.Price * order.Amount;
                if (order.Side == Side.Buy)
                {
                    Profit += totalAmount;
                }
                if (order.Side == Side.Sell)
                {
                    Profit -= totalAmount;
                }
            };

            // Act
            foreach (var order in Orders)
            {
                Sut.Place(order);
            }
        }
    }
}
