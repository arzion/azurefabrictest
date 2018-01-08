using System;
using System.Collections.Generic;

namespace Wirex.Engine.Services
{
    /// <summary>
    /// The storage of the orders that uses linked dictionary to store and find orders.
    /// </summary>
    /// <seealso cref="Wirex.Engine.Services.IOrdersStorage" />
    public class LinkedDictionaryOrdersStorage : IOrdersStorage
    {
        private readonly IOrdersMatchingRule _ordersMatchingRule;
        private readonly LinkedOrders _orders = new LinkedOrders();

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedDictionaryOrdersStorage"/> class.
        /// </summary>
        /// <param name="ordersMatchingRule">The orders matching rule.</param>
        public LinkedDictionaryOrdersStorage(IOrdersMatchingRule ordersMatchingRule)
        {
            _ordersMatchingRule = ordersMatchingRule;
        }

        private readonly object _sync = new object();

        /// <summary>
        /// Adds the specified order into the storage.
        /// </summary>
        /// <param name="order">The order to add.</param>
        public void Add(Order order)
        {
            lock (_sync)
            {
                _orders.AddToEnd(order);
            }
        }

        /// <summary>
        /// Removes the specified order from the storage.
        /// </summary>
        /// <param name="order">The order to remove.</param>
        public void Remove(Order order)
        {
            lock (_sync)
            {
                _orders.Remove(order);
            }
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
            lock (_sync)
            {
                var currentLinkedOrder = current == null
                    ? _orders.First()
                    : _orders[current.Id];

                if (_ordersMatchingRule.IsMatched(currentLinkedOrder.Order, target))
                {
                    return currentLinkedOrder.Order;
                }

                if (!currentLinkedOrder.Next.HasValue)
                {
                    return null;
                }

                Guid? foundId = currentLinkedOrder.Next.Value;
                while (!_ordersMatchingRule.IsMatched(_orders[foundId.Value].Order, target))
                {
                    foundId = _orders[foundId.Value].Next;
                    if (!foundId.HasValue)
                    {
                        break;
                    }
                }

                return !foundId.HasValue ? null : _orders[foundId.Value].Order;
            }
        }

        #region nested

        private class LinkedOrders
        {
            private readonly Dictionary<Guid, LinkedOrder> _linkedOrders = new Dictionary<Guid, LinkedOrder>();
            private Guid? _first;
            private Guid? _last;

            public LinkedOrder this[Guid value] => _linkedOrders[value];

            /// <summary>
            /// Gets the first item of linked orders.
            /// </summary>
            /// <returns>The first item of the orders.</returns>
            /// <exception cref="InvalidOperationException">Collection has no items yet.</exception>
            public LinkedOrder First()
            {
                if (!_first.HasValue)
                {
                    throw new InvalidOperationException("Collection has no items yet");
                }
                return _linkedOrders[_first.Value];
            }

            /// <summary>
            /// Adds the order to the end of linked list.
            /// </summary>
            /// <param name="order">The order to add.</param>
            public void AddToEnd(Order order)
            {
                // if it is first item - init _first
                if (!_first.HasValue)
                {
                    _first = order.Id;
                }

                // add order into the dictionary
                _linkedOrders[order.Id] = new LinkedOrder
                {
                    Order = order,
                    Next = null,
                    Prev = _last
                };

                // if there is last - add next link of last to current order
                if (_last.HasValue)
                {
                    _linkedOrders[_last.Value].Next = order.Id;
                }

                // init _last as current order
                _last = order.Id;
            }

            /// <summary>
            /// Removes the specified order from the linked list.
            /// </summary>
            /// <param name="order">The order to remove.</param>
            public void Remove(Order order)
            {
                if (!_linkedOrders.ContainsKey(order.Id))
                {
                    return;
                }

                var linkedOrderToRemove = _linkedOrders[order.Id];

                // link prev to the next
                if (linkedOrderToRemove.Prev.HasValue)
                {
                    _linkedOrders[linkedOrderToRemove.Prev.Value].Next = linkedOrderToRemove.Next;
                }

                // link next to prev
                if (linkedOrderToRemove.Next.HasValue)
                {
                    _linkedOrders[linkedOrderToRemove.Next.Value].Prev = linkedOrderToRemove.Prev;
                }

                // change _first and _last if this is the order to remove
                if (_first == linkedOrderToRemove.Order.Id)
                {
                    _first = linkedOrderToRemove.Next;
                }
                if (_last == linkedOrderToRemove.Order.Id)
                {
                    _last = linkedOrderToRemove.Prev;
                }

                // removing order from the linked list
                _linkedOrders.Remove(order.Id);
            }
        }

        private class LinkedOrder
        {
            public Guid? Next { get; set; }

            public Guid? Prev { get; set; }

            public Order Order { get; set; }
        }

        #endregion
    }
}
