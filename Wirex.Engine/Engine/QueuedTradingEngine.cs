using System;
using Wirex.Engine.Services;

namespace Wirex.Engine.Engine
{
    /// <summary>
    /// Represents the engine of trading operations.
    /// </summary>
    public class QueuedTradingEngine : ITradingEngine
    {
        private readonly IOrdersMatchingRule _ordersMatchingRule;
        private readonly OrderQueueStorage _orderQueueStorage;

        private readonly object _processOrdersSyncObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedTradingEngine"/> class.
        /// </summary>
        /// <param name="ordersMatchingRule">The orders matching rule.</param>
        public QueuedTradingEngine(IOrdersMatchingRule ordersMatchingRule)
        {
            _ordersMatchingRule = ordersMatchingRule;
            _orderQueueStorage = new OrderQueueStorage();
        }

        /// <summary>
        /// Places the specified order to process.
        /// </summary>
        /// <param name="order">The order.</param>
        public void Place(Order order)
        {
            lock (_processOrdersSyncObject)
            {
                var queue = _orderQueueStorage.GetOrCreateQueue(order);
                queue.Add(order);
            }

            RaiseOrderOpened(order);

            CloseOrders(order);
        }

        /// <summary>
        /// Occurs when order is opened.
        /// </summary>
        public event EventHandler<OrderArgs> OrderOpened;

        /// <summary>
        /// Occurs when order is closed.
        /// </summary>
        public event EventHandler<OrderArgs> OrderClosed;

        private void CloseOrders(Order order)
        {
            OrderQueue oppositeQueue;
            lock (_processOrdersSyncObject)
            {
                oppositeQueue = _orderQueueStorage.GetOrCreateOppositeQueue(order);
                if (oppositeQueue.IsEmpty)
                {
                    return;
                }
            }

            while (true)
            {
                Order oppositeOrder;
                lock (_processOrdersSyncObject)
                {
                    oppositeOrder = oppositeQueue.Peek();
                }
                if (oppositeOrder == null)
                {
                    break;
                }

                var matched = _ordersMatchingRule.IsMatched(order, oppositeOrder);

                if (matched)
                {
                    lock (_processOrdersSyncObject)
                    {
                        matched = _ordersMatchingRule.IsMatched(order, oppositeOrder);
                        if (matched)
                        {
                            ProcessOrders(order, oppositeOrder);
                        }
                        else
                        {
                            // process further if some thread is here
                            continue;
                        }
                    }
                }

                lock (_processOrdersSyncObject)
                {
                    if (order.IsClosed || !matched || oppositeQueue.IsEmpty)
                    {
                        break;
                    }
                }
            }
        }

        private void ProcessOrders(Order order1, Order order2)
        {
            var amountToSubtract = Math.Min(order1.RemainingAmount, order2.RemainingAmount);
            order1.RemainingAmount -= amountToSubtract;
            order2.RemainingAmount -= amountToSubtract;

            if (order1.IsClosed)
            {
                _orderQueueStorage.GetOrCreateQueue(order1).Remove(order1);
                RaiseOrderClosed(order1);
            }
            if (order2.IsClosed)
            {
                _orderQueueStorage.GetOrCreateQueue(order2).Remove(order2);
                RaiseOrderClosed(order2);
            }
        }

        private void RaiseOrderClosed(Order order)
        {
            OrderClosed?.Invoke(this, new OrderArgs(order));
        }

        private void RaiseOrderOpened(Order order)
        {
            OrderOpened?.Invoke(this, new OrderArgs(order));
        }
    }
}