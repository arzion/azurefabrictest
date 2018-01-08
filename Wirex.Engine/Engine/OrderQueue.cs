using System;
using System.Collections.Generic;
using System.Linq;

namespace Wirex.Engine.Engine
{
    /// <summary>
    /// Represents the queue of orders with same settings.
    /// </summary>
    public class OrderQueue
    {
        private readonly SortedList<OrderSortedKey, Order> _orders;

        /// <summary>
        /// The currency pair of orders in queue.
        /// </summary>
        public CurrencyPair CurrencyPair { get; }

        /// <summary>
        /// Gets or sets the side of orders in queue.
        /// </summary>
        public Side Side { get; }

        /// <summary>
        /// Gets a value indicating whether the queue is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the queue is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => !_orders.Any();

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderQueue"/> class.
        /// </summary>
        /// <param name="currencyPair">The currency pair.</param>
        /// <param name="side">The side.</param>
        public OrderQueue(CurrencyPair currencyPair, Side side)
        {
            CurrencyPair = currencyPair;
            Side = side;
            _orders = new SortedList<OrderSortedKey, Order>();
        }

        /// <summary>
        /// Adds the order into the queue.
        /// </summary>
        /// <param name="order">The order that should be placed into the queue.</param>
        public void Add(Order order)
        {
            if (!Equals(order.CurrencyPair, CurrencyPair))
            {
                throw new InvalidOperationException("Order cannot be added into the queue because it has different currency pair." +
                    $"Order currency pair is: {order.CurrencyPair} But queue is for {CurrencyPair}");
            }

            if (order.Side != Side)
            {
                throw new InvalidOperationException("Order cannot be added into the queue because it has different side." +
                    $"Order side is: {order.Side} But queue is for {Side}");
            }

            _orders.Add(new OrderSortedKey(order), order);
        }

        /// <summary>
        /// Removes order from the queue.
        /// </summary>
        public void Remove(Order order)
        {
            _orders.Remove(new OrderSortedKey(order));
        }

        /// <summary>
        /// Take last order from the queue but without removing it from the queue.
        /// </summary>
        /// <returns>
        /// Order with max-min price according to side or null if queue is emoty.
        /// </returns>
        /// <exception cref="InvalidOperationException"> No items inside collection, not able to peek.</exception>
        public Order Peek()
        {
            if (IsEmpty)
            {
                return null;
            }

            // take order with lowest price if order is for sell
            if (Side == Side.Sell)
            {
                var firstOrder = _orders.FirstOrDefault();
                return firstOrder.Value;
            }

            // otherwise take order with highest price
            var lastOrder = _orders.LastOrDefault();
            return lastOrder.Value;
        }

        private struct OrderSortedKey : IComparable<OrderSortedKey>, IComparable
        {
            private readonly decimal _price;
            private readonly Guid _orderId;

            public OrderSortedKey(Order order)
            {
                _price = order.Price;
                _orderId = order.Id;
            }

            #region comparing

            public int CompareTo(object obj)
            {
                return CompareTo((OrderSortedKey) obj);
            }

            public int CompareTo(OrderSortedKey other)
            {
                var priceComparison = _price.CompareTo(other._price);
                if (priceComparison != 0) return priceComparison;
                return _orderId.CompareTo(other._orderId);
            }

            #endregion

            #region equality

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                return obj.GetType() == GetType() && Equals((OrderSortedKey)obj);
            }

            private bool Equals(OrderSortedKey other)
            {
                return Guid.Equals(_orderId, other._orderId) && decimal.Equals(_price, other._price);
            }

            public override int GetHashCode()
            {
                return _price.GetHashCode();
            }

            #endregion
        }
    }
}
