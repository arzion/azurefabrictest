using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirex.Engine.Engine;
using Wirex.Engine.Services;

namespace Wirex.Engine.Tests.Given_TradingEngine
{
    public class RandomOrdersProcessedInParallelClassFixture : ClassFixtureBase
    {
        public IList<Order> Orders { get; set; }
        public IList<Order> ActualOpenedOrders { get; set; }
        public IList<Order> ActualClosedOrders { get; set; }

        public ITradingEngine Sut { get; set; }

        public RandomOrdersProcessedInParallelClassFixture()
        {
            // Arrange
            Orders = GetRandomOrders();
            ActualOpenedOrders = new List<Order>();
            ActualClosedOrders = new List<Order>();

            var concurrentOrders = new ConcurrentQueue<Order>(Orders);

            Sut = new QueuedTradingEngine(new OrdersMatchingRule());

            Sut.OrderOpened += (sender, args) => { ActualOpenedOrders.Add(args.Order); };
            Sut.OrderClosed += (sender, args) => { ActualClosedOrders.Add(args.Order); };

            const int threadCount = 5;

            //Simulate multi-threading environment
            var tasks = new List<Task>();
            for (var i = 0; i < threadCount; i++)
            {
                tasks.Add(Task.Run(() => PlaceOrder(Sut, concurrentOrders)));
            }

            Task.WhenAll(tasks).Wait();
        }

        private static void PlaceOrder(ITradingEngine engine, ConcurrentQueue<Order> orders)
        {
            while (orders.TryDequeue(out var order))
            {
                engine.PlaceAsync(order);
            }
        }
    }
}
