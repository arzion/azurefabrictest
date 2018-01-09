using System.Collections.Generic;
using System.Linq;

namespace Wirex.Engine.Engine
{
    /// <summary>
    /// Represents the queue of orders with same settings.
    /// </summary>
    public class OrderQueueStorage
    {
        private readonly  IList<OrderQueue> _queues;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderQueueStorage"/> class.
        /// </summary>
        public OrderQueueStorage()
        {
            _queues = new List<OrderQueue>();
        }

        /// <summary>
        /// Gets or creates the order queue.
        /// </summary>
        /// <param name="currencyPair">The currency pair.</param>
        /// <param name="side">The side.</param>
        /// <returns>Order queue for currency pair and side.</returns>
        public OrderQueue GetOrCreateQueue(CurrencyPair currencyPair, Side side)
        {
            var foundQueue =
                _queues.FirstOrDefault(q => Equals(q.Key.CurrencyPair, currencyPair) && q.Key.Side == side);

            if (foundQueue == null)
            {
                var newQueue = new OrderQueue(currencyPair, side);
                _queues.Add(newQueue);
                return newQueue;
            }

            return foundQueue;
        }

        /// <summary>
        /// Gets or creates the order queue by order parameters.
        /// </summary>
        /// <returns>Order queue that is applicable for the order.</returns>
        public OrderQueue GetOrCreateQueue(Order order)
        {
            return GetOrCreateQueue(order.CurrencyPair, order.Side);
        }

        public OrderQueue GetOrCreateOppositeQueue(Order order)
        {
            var oppositeSide = order.Side == Side.Buy ? Side.Sell : Side.Buy;
            return GetOrCreateQueue(order.CurrencyPair, oppositeSide);
        }
    }
}
