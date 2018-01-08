using System;
using System.Collections.Generic;

namespace Wirex.Engine.Services
{
    /// <summary>
    /// The order storage that is used full-concurrent list as the storage of orders.
    /// </summary>
    /// <seealso cref="Wirex.Engine.Services.IOrdersStorage" />
    public class ConcurrentListOrdersStorage : IOrdersStorage
    {
        private readonly IOrdersMatchingRule _ordersMatchingRule;
        private readonly ConcurrentOrders _orders = new ConcurrentOrders();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentListOrdersStorage"/> class.
        /// </summary>
        /// <param name="ordersMatchingRule">The orders matching rule.</param>
        public ConcurrentListOrdersStorage(IOrdersMatchingRule ordersMatchingRule)
        {
            _ordersMatchingRule = ordersMatchingRule;
        }

        /// <summary>
        /// Adds the specified order into the storage.
        /// </summary>
        /// <param name="order">The order to add.</param>
        public void Add(Order order)
        {
            _orders.Add(order);
        }

        /// <summary>
        /// Removes the specified order from the storage.
        /// </summary>
        /// <param name="order">The order to remove.</param>
        public void Remove(Order order)
        {
            _orders.Remove(order);
        }

        /// <summary>
        /// Gets the next matched order according to matching rules.
        /// </summary>
        /// <param name="current">The current order to start search from.</param>
        /// <param name="target">The target order for which matching rules should be applied.</param>
        /// <returns>
        /// Found order that is matched with target, or null if there is no such.
        /// </returns>
        public Order GetNextMatched(Order current, Order target)
        {
            for (var i = current == null
                    ? 0
                    : _orders.IndexOf(current); i < _orders.Count; i++)
            {
                Order placedOrder;
                try
                {
                    placedOrder = _orders[i];
                }
                catch (IndexOutOfRangeException)
                {
                    // order can be removed during itteration
                    return null;
                }

                if (_ordersMatchingRule.IsMatched(placedOrder, target))
                {
                    return placedOrder;
                }
            }

            return null;
        }

        #region nested

        private class ConcurrentOrders
        {
            private readonly IList<Order> _orders = new List<Order>();
            private readonly object _sync = new object();

            public int Count
            {
                get
                {
                    lock (_sync)
                    {
                        return _orders.Count;
                    }
                }
            }

            public Order this[int index]
            {
                get
                {
                    lock (_sync)
                    {
                        return _orders[index];
                    }
                }
            }

            public void Add(Order order)
            {
                lock (_sync)
                {
                    _orders.Add(order);
                }
            }

            public void Remove(Order order)
            {
                lock (_sync)
                {
                    _orders.Remove(order);
                }
            }

            public int IndexOf(Order order)
            {
                lock (_sync)
                {
                    return _orders.IndexOf(order);
                }
            }
        }

        #endregion
    }
}
