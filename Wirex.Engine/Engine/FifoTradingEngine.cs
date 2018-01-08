using System;
using Wirex.Engine.Services;

namespace Wirex.Engine.Engine
{
    /// <summary>
    /// Represents the engine of trading operations that closes the earliest orders first.
    /// </summary>
    /// <seealso cref="ITradingEngine" />
    public class FifoTradingEngine : ITradingEngine
    {
        private readonly IOrdersStorage _ordersStorage;

        private readonly object _processOrdersSyncObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="FifoTradingEngine"/> class.
        /// </summary>
        /// <param name="ordersStorage">The orders storage.</param>
        public FifoTradingEngine(IOrdersStorage ordersStorage)
        {
            _ordersStorage = ordersStorage;
        }

        /// <summary>
        /// Places the specified order to process.
        /// </summary>
        /// <param name="order">The order.</param>
        public void Place(Order order)
        {
            PlaceOrder(order);
            ProcessAll(order);
        }

        /// <summary>
        /// Occurs when order is opened.
        /// </summary>
        public event EventHandler<OrderArgs> OrderOpened;

        /// <summary>
        /// Occurs when order is closed.
        /// </summary>
        public event EventHandler<OrderArgs> OrderClosed;

        private void PlaceOrder(Order order)
        {
            _ordersStorage.Add(order);
            RaiseOrderOpened(order);
        }

        private void ProcessAll(Order order)
        {
            Order applicableOrder;
            while ((applicableOrder = _ordersStorage.GetNextMatched(null, order)) != null)
            {
                lock (_processOrdersSyncObject)
                {
                    ProcessOrders(applicableOrder, order);
                }

                if (order.IsClosed)
                {
                    break;
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
                _ordersStorage.Remove(order1);
                RaiseOrderClosed(order1);
            }
            if (order2.IsClosed)
            {
                _ordersStorage.Remove(order2);
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